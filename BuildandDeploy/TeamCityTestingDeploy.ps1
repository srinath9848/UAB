param($deployNum, $buildNum)

$destComputers = @("VD-TSTPC10-WEB") # Testing Environment Server (Web Server), Hosted the ProCoder Reporting application in this server. 
$retentionPeriod = New-TimeSpan -Days 2
$retentionCount = 5

#normally set in base file
# Be sure to include the trailing slash on the following paths
$webPath = "W:\Websites\UAB1\"
$siteAndVDir = "UAB1/"
$logPath = "W:\Logs\UAB1\"

# For returning the script's exit code as the Powershell.exe process exit code: http://thepowershellguy.com/blogs/posh/archive/2008/05/20/hey-powershell-guy-how-can-i-run-a-powershell-script-from-cmd-exe-and-return-an-errorlevel.aspx
# Handle all exceptions thrown during the script and exit with a non-success exit code
trap
{
	write-output $_
	exit 1
}

# Inform powershell that all errors should immediately stop the script execution
$ErrorActionPreference = 'Stop';
$crystalReportLibrariesPath = 'C:\inetpub\wwwroot\aspnet_client'

# Inform powershell that the output buffer is very wide rather than the default console window width.
if( $Host -and $Host.UI -and $Host.UI.RawUI ) {
 $rawUI = $Host.UI.RawUI
 $oldSize = $rawUI.BufferSize
 $typeName = $oldSize.GetType( ).FullName
 $newSize = New-Object $typeName (1000, $oldSize.Height)
 $rawUI.BufferSize = $newSize
}

function getUnc ($machine, $path) {
    return "\\$machine\$($path -replace ':', '$')"
}

function RepointIIS($webServer, $physPath, $siteAndVDir)
{
 $session = new-pssession -computername $webServer
    Invoke-Command -session $session -ScriptBlock {
    	param([string] $physPath, [string] $siteAndVDir)
    	$ErrorActionPreference = 'Stop';
    	$success = $false

        #need to send the "right" set of `quotes` in case $siteAndVDir has spaces or $physPath has spaces
        c:\windows\system32\inetsrv\appcmd.exe set vdir `"$siteAndVDir`" `"/physicalPath:$physPath`" | Write-Host
    	if($lastexitcode -ne 0) {
    		return 
    	}
    	Write-Host "New path is $physPath"
    	$success = $true
    } -argumentList $physPath, $siteAndVDir
    $success = Invoke-Command -session $session -ScriptBlock {$success}
    remove-pssession $session
    
    return $success
}  

$targetFolder = "D$deployNum.B$buildNum"

# copy the web files to the new directory on each server
foreach ($webServer in $destComputers) {
    # construct UNC path
    $destUnc = (getUnc $webServer $webPath) + $targetFolder
    # create new dir
    mkdir $destUnc > $null
    # copy the files
    copy-item '\ToDeploy\Web\*' $destUnc -recurse -PassThru | select-object FullName
	$UncCrystalReportLibrariesPath = (getUnc $webServer $crystalReportLibrariesPath)
	# copy the crystal reports runtime files
	copy-item $UncCrystalReportLibrariesPath $destUnc -recurse -PassThru | select-object FullName
}

$webPathForCleanUp = $webPath

# switch the web root directory to the new version
foreach ($webServer in $destComputers) {
    echo "Upgrading web site on $webServer"
    
    $success = RepointIIS $webServer "$webPath$targetFolder" $siteAndVDir
    
    if($success -ne $true) {
        throw "Failed to upgrade the website on $webServer."
    }
}

# cleanup old deployments
foreach($webServer in $destComputers){
    #delete old website deployments
    echo "cleaning up folders on $webServer"
    $folderList = ls $(getUnc $webServer $webPathForCleanUp) D*.B* |
        sort  CreationTime -Descending |
        select -skip ($retentionCount +1) |
        where {$_.CreationTime -le ($(get-date) - $retentionPeriod)}
    foreach($folder in $folderList ) {
        echo "Removing $($folder.FullName)"
        rmdir $folder.FullName -Recurse -Force #-ea SilentlyContinue
    }
}

exit $lastexitcode
