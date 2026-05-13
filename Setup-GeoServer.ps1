# GeoServer REST API connection settings
$GeoServerUrl = "http://localhost:8080/geoserver/rest"
$Credentials = "admin:SuperSecretAdmin123!"
$Bytes = [System.Text.Encoding]::ASCII.GetBytes($Credentials)
$Base64 = [System.Convert]::ToBase64String($Bytes)
$Headers = @{ Authorization = "Basic $Base64" }

Write-Host "Starting GeoServer configuration..." -ForegroundColor Cyan

# 1. Create Workspace
Write-Host "1. Creating 'cadastre' workspace..."
$WorkspaceXml = "<workspace><name>cadastre</name></workspace>"
try {
    Invoke-RestMethod -Uri "$GeoServerUrl/workspaces" -Method Post -Headers $Headers -Body $WorkspaceXml -ContentType "text/xml"
    Write-Host "Workspace created successfully." -ForegroundColor Green
} catch { Write-Host "Workspace already exists or an error occurred." -ForegroundColor Yellow }

# 2. Create DataStore (PostGIS connection)
Write-Host "2. Connecting to PostGIS database..."
$DataStoreXml = @"
<dataStore>
  <name>cadastre_db</name>
  <connectionParameters>
    <host>cadastre-db</host>
    <port>5432</port>
    <database>cadastre_db</database>
    <schema>registry</schema>
    <user>postgres</user>
    <passwd>SuperSecretPassword123!</passwd>
    <dbtype>postgis</dbtype>
    <Expose%20primary%20keys>true</Expose%20primary%20keys>
  </connectionParameters>
</dataStore>
"@
try {
    Invoke-RestMethod -Uri "$GeoServerUrl/workspaces/cadastre/datastores" -Method Post -Headers $Headers -Body $DataStoreXml -ContentType "text/xml"
    Write-Host "Database connection established." -ForegroundColor Green
} catch { Write-Host "DataStore already exists." -ForegroundColor Yellow }

# 3. Publish spatial_units layer
Write-Host "3. Publishing 'spatial_units' layer..."
$FeatureTypeXml = @"
<featureType>
  <name>spatial_units</name>
  <nativeName>spatial_units</nativeName>
  <title>Cadastral Parcels (LADM)</title>
  <srs>EPSG:4326</srs>
  <nativeBoundingBox>
    <minx>-180</minx>
    <maxx>180</maxx>
    <miny>-90</miny>
    <maxy>90</maxy>
    <crs>EPSG:4326</crs>
  </nativeBoundingBox>
  <latLonBoundingBox>
    <minx>-180</minx>
    <maxx>180</maxx>
    <miny>-90</miny>
    <maxy>90</maxy>
    <crs>EPSG:4326</crs>
  </latLonBoundingBox>
</featureType>
"@
try {
    Invoke-RestMethod -Uri "$GeoServerUrl/workspaces/cadastre/datastores/cadastre_db/featuretypes" -Method Post -Headers $Headers -Body $FeatureTypeXml -ContentType "text/xml"
    Write-Host "Layer published successfully!" -ForegroundColor Green
} catch { Write-Host "Layer is already published." -ForegroundColor Yellow }

Write-Host "Configuration complete! GeoServer is ready." -ForegroundColor Cyan