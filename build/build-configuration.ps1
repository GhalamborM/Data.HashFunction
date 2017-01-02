$baseDir  = Resolve-Path $PSScriptRoot\.. -Relative
$currentYear = [DateTime]::Today.Year

properties {
	$configuration = "Debug"
	$nuGetBaseUrl = "https://www.nuget.org/api/v2/"
	$preReleaseTag = "local"
	$signAssemblies = $false
	$signKeyPath = "$baseDir\Data.HashFunction.Production.pfx"
	$treatWarningsAsErrors = $true
  
	$gitExecutable = "git"
	$dotNetExecutable = "dotnet"

	$artifactsDir = "$baseDir\Artifacts"
	$buildDir = "$baseDir\build"
	$sourceDir = "$baseDir\src"
	$toolsDir = "$baseDir\tools"
	$releaseDir = "$baseDir\release"
	$nuGetDir = "$baseDir\NuGet"

	$buildNumber = Read-BuildNumber "$buildDir\BuildNumber.txt"
}


Task Default -depends Validate,Build,Test

Task Validate -depends Validate-Versions
Task Build -depends Build-Solution
Task Test -depends Test-Solution

Task Load-Powershell-Dependencies {
	Add-Type -Path "$sourceDir\packages\NuGet.Core.2.14.0\lib\net40-Client\NuGet.Core.dll"
}

Task Resolve-Projects -depends Load-Powershell-Dependencies {
	$script:projects = [System.Collections.ArrayList]::new()
	
	$projectDirectories = Get-ChildItem "$sourceDir\System.Data.HashFunction.*" -Directory
	foreach ($projectDirectory in $projectDirectories)
	{
		$name = $projectDirectory.Name

		$projectObject = Get-Content "$sourceDir\$name\project.json" | ConvertFrom-Json

		$versionSuffix = ""

		if ($preReleaseTag -ne "")
		{
			$versionSuffix = "$preReleaseTag-$buildNumber"
		}


		$project = New-Object �TypeName PSObject �Prop @{
			Name = $name
			Path = "$sourceDir\$name"
			ProjectJsonPath = "$sourceDir\$name\project.json"
			Project = $projectObject
			SemanticVersion = [NuGet.SemanticVersion]::new($projectObject.version.Replace("-*", "-$versionSuffix"))
			VersionSuffix = $versionSuffix
			NuGetPath = "$nuGetDir\$name"
			NuGetPackageName = $name
			SkipPackaging = $name -eq "System.Data.HashFunction.Test"
			RunTests = $name -eq "System.Data.HashFunction.Test"
		}

		$script:projects.Add($project) > $null
	}
}

Task Resolve-Production-Versions -depends Load-Powershell-Dependencies,Resolve-Projects {
	foreach ($project in $script:projects)
	{
		if ($project.SkipPackaging)
		{
			continue
		}

		$xmlDocument = [System.Xml.XmlDocument]::new()
			
		$versionResults = Invoke-WebRequest $($nuGetBaseUrl + "FindPackagesById()?Id='" + $project.NuGetPackageName + "'")
		$xmlDocument.LoadXml($versionResults.Content)

		$versions = New-Object -TypeName PSObject -Property @{
			Production = $(New-Object -TypeName PSObject -Property @{
				SemanticVersion = $null
				VcsRevision = $null
			})
			PreRelease = $(New-Object -TypeName PSObject -Property @{
				SemanticVersion = $null
				VcsRevision = $null
			}) 
		}

		foreach ($entry in $xmlDocument.feed.entry)
		{
			$entrySemanticVersion = [NuGet.SemanticVersion]::new($entry.properties.Version)
			$entryVcsRevision = Parse-VCS-Revision $entry.properties.ReleaseNotes

			if ($entry.properties.IsLatestVersion)
			{
				$versions.Production.SemanticVersion = $entrySemanticVersion
				$versions.Production.VcsRevision = $entryVcsRevision
			}

			if ($entry.properties.IsAbsoluteLatestVersion)
			{
				$versions.PreRelease.SemanticVersion = $entrySemanticVersion
				$versions.PreRelease.VcsRevision = $entryVcsRevision
			}
		}

	    Add-Member -InputObject $project -MemberType NoteProperty -Name "Versions" -Value $versions
	}
}

Task Validate-Versions -depends Resolve-Production-Versions {
	$anyVersionBumpRequired = $false

	foreach ($project in $script:projects)
	{
		if ($project.SkipPackaging)
		{
			continue
		}
		

		Write-Host $("Validating version for " + $project.Name + ".")


		$packageRevision = $project.NewestVersionVcsRevision

		if ($versions.Production.Version -ne $null -and $versions.Production.SemanticVersion.Version -gt $project.SemanticVersion.Version)
		{
			Write-Host "Newer production version already exists, version bump required." -ForegroundColor Red
			$anyVersionBumpRequired = $true;

		} else  {
			if ($project.NewestVersionVcsRevision -ne $null)
			{
				$fileChanges = @()

				if ($packageRevision -ne $null)
				{
					$fileChanges = $(& $gitExecutable diff $packageRevision $project.Path)
				}

				if ($fileChanges.Count -gt 0)
				{
					Write-Host "Changes made since last production deploy, version bump required." -ForegroundColor Red
					$anyVersionBumpRequired = $true
				}
			}
		}
	}

	if ($anyVersionBumpRequired)
	{
		throw "Version bump required for at least one project."
	}
}

task Build-Solution -depends Resolve-Projects {	
	
	$vcsRevision = Exec { & $gitExecutable rev-parse HEAD }


	if (Test-Path $artifactsDir)
	{
		Remove-Item "$artifactsDir\*" -Force
	}

	$allProjects = [System.Collections.ArrayList]::new()

	try
	{

		foreach ($project in $script:projects)
		{
			$projectJsonPath = $project.Path + "\project.json"
			$oldProjectJsonPath = $project.Path + "\project.old.json"


			if (!$project.SkipPackaging)
			{
				#if (Test-Path $oldProjectJsonPath)
				#{
				#	Move-Item $oldProjectJsonPath -Destination $projectJsonPath -Force
				#	throw "project.old.json exists!"
				#}

				#Copy-Item $projectJsonPath -Destination $oldProjectJsonPath

				#$updatedProject = $project.Project | ConvertTo-Json -Depth 100 | ConvertFrom-Json
				#$updatedProject.packOptions.releaseNotes += "`nvcs-revision: $vcsRevision"
			
				#Set-Content $projectJsonPath -Value $(ConvertTo-Json $updatedProject -Depth 100) -Force
			}

			$allProjects.Add($projectJsonPath) > $null
		}


		Exec { & $dotNetExecutable restore $allProjects > $null }
	
		if ($project.VersionSuffix -ne "")
		{
			Exec { & $dotNetExecutable build $allProjects -c $configuration --version-suffix $project.VersionSuffix }

		} else {
			Exec { & $dotNetExecutable build $allProjects -c $configuration }
		}
		
		
		foreach ($project in $script:projects)
		{
			$projectJsonPath = $project.Path + "\project.json"

			if ($project.VersionSuffix -ne "")
			{
				Exec { & $dotNetExecutable pack $projectJsonPath -c $configuration --version-suffix $project.VersionSuffix -o $artifactsDir --no-build  }
			} else {
				Exec { & $dotNetExecutable pack $projectJsonPath -c $configuration -o $artifactsDir }
			}
		}
	} finally {
		#foreach ($project in $script:projects)
		#{
		#	$projectJsonPath = $project.Path + "\project.json"
		#	$oldProjectJsonPath = $project.Path + "\project.old.json"


		#	if (Test-Path $oldProjectJsonPath)
		#	{
		#		Move-Item $oldProjectJsonPath -Destination $projectJsonPath -Force
		#	}
		#}
	}
}

task Test-Solution -depends Resolve-Projects {
	foreach ($project in $script:projects)
	{
		if ($project.RunTests)
		{
			$projectJsonPath = $project.Path + "\project.json"

			Exec { & $dotNetExecutable test "$projectJsonPath" }
		}
	}
}

function Read-BuildNumber {
	param (
		[string] $buildNumberFilePath
	)

	$buildNumber = 0

	if (-Not (Test-Path $buildNumberFilePath))
	{
		New-Item $buildNumberFilePath -ItemType File -Value $buildNumber
	}


	$buildNumber = [int]::Parse($(Get-Content $buildNumberFilePath -Raw)) + 1

	Set-Content $buildNumberFilePath -Value $buildNumber

	return $buildNumber
}

function Parse-VCS-Revision
{
	param (
		[string] $releaseNotes
	)

	[System.Text.RegularExpressions.Match] $match = [Regex]::Match($releaseNotes, "vcs-revision: ([0-9a-fA-F]{32})")

	if (!$match.Success)
	{
		return $null
	}

	return $match.Groups[1]
}
