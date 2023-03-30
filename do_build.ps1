param (
	$Commit = $null,
	$Version = $null,
	[Switch] $VersionAsTag = $false,
	[Switch] $VersionFromTag = $false
)

$devloopUrl = "https://github.com/devlooped/nugetizer.git"
$baseDir = $PSScriptRoot

if ($VersionAsTag) {
	if ($Version -eq $null) {
		Write-Error "Version must be specified if VersionAsTag is used"
		return 2
	}
	$Commit = "v$Version"
} elseif ($VersionFromTag) {
	if ($Commit -eq $null) {
		Write-Error "Commit must be specified if VersionFromTag is used"
		return 4
	}
	if ($Commit -match "v(\d\.\d\.\d.*)") {
		$Version = $Matches[1]
	} else {
		Write-Error "Commit does not appear to be a typical version tag"
		return 4
	}
}

if ($Commit -ne $null -and $Version -ne $null) {
	Write-Host "Pulling commit $Commit and building as version $Version"
} elseif ($Commit -ne $null) {
	Write-Host "Pulling commit $Commit and building with default version"
} elseif ($Versuib -ne $null) {
	Write-Host "Pulling main branch and building as version $Version"
} else {
	Write-Host "Pulling main branch and building with default version"
}

cd $baseDir

function Fetch-Commit {
	param (
		$Commit = $null
	)
	
	$x = git status -s
	#write-host "status" $?
	if (!$?) { return $false }
	git remote set-url origin $devloopUrl
	#write-host "remote set-url" $?
	if (!$?) { return $false }
	
	$fetchArgs = @("-ptf", "--depth=1")
	if ($Commit -ne $null) {
		git fetch origin $Commit $fetchArgs
	} else {
		git pull origin main $fetchArgs
	}
	#write-host "fetch" $?
	if (!$?) { return $false }
	return $true
}

$shouldClone = $true;
if (Test-Path -PathType Container .repo) {
	pushd .repo
	$didFetch = Fetch-Commit -Commit $Commit
	$shouldClone = !$didFetch
	popd
	#write-host "shouldClone" $shouldClone
	if ($shouldClone) {
		rm -r -force .repo
	}
}

if ($shouldClone) {
	Write-Host initing repo
	git init -q .repo
	pushd .repo
	git remote add origin $devloopUrl
	$didFetch = Fetch-Commit -Commit $Commit
	popd
	if (!$didFetch) {
		Write-Error "Could not fetch requested commit"
		return 1
	}
}

pushd .repo
&{
	if ($Commit -ne $null) {
		git checkout -f --detach $Commit
		if (!$?) { return } # exit early
	}
	
	git apply ../unsponsor.patch --ignore-whitespace --recount
	if (!$?) { return } # exit early
	
	$props = @("-c","Release","-p:BuildPackageBaseName=UnNuGetizer","-p:BuildPackageBaseName2=unnugetize")
	if ($Version -ne $null) {
		$props += @("-p:VersionPrefix=$Version")
	}
	
	dotnet build $props
	if (!$?) { return } # exit early
	dotnet pack $props
	if (!$?) { return } # exit early
}
popd