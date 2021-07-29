param($deployNum, $buildNum)

$destComputers = @("VD-TSTPC10-WEB") # Testing Environment Server (Web Server), Hosted the ProCoder Reporting application in this server. 
$retentionPeriod = New-TimeSpan -Days 2
$retentionCount = 5

#normally set in base file
# Be sure to include the trailing slash on the following paths
$webPath = "W:\Websites\UAB1\"
$siteAndVDir = "UAB1/"
$logPath = "W:\Logs\UAB1\"

.\TeamCityDeployBase.ps1
exit $lastexitcode
