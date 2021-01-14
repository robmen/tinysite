@setlocal
@pushd %~dp0

dotnet publish -c Release src\tinysite
dotnet pack -c Release src\tinysite.pack

@popd
@endlocal
