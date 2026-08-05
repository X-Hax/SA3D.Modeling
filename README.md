[![NuGet](https://img.shields.io/nuget/v/SA3D.Modeling)](https://www.nuget.org/packages/SA3D.Modeling) 
[![downloads](https://img.shields.io/nuget/dt/SA3D.Modeling)](https://www.nuget.org/packages/SA3D.Modeling)

# SA3D.Modeling
A Sonic Adventure modeling library with support for all game related model formats. Also contains support for various other SEGA based games, although support is not guaranteed.

## Contents
| Namespace (SA3D.Modeling.*) 	| Description                                                                           |
|-----------------------------	|---------------------------------------------------------------------------------------|
| File                        	| Model data storage file handlers for select native- and X-Hax custom file-formats.    |
| Mesh                        	| Library for handling, reading and writing mesh data.                                  |
| Mesh.Basic                  	| Basic mesh data library. Used in SA1 (everything) and SA2 (collision geometry only)   |
| Mesh.Chunk                  	| Chunk mesh data library. Used in SA2.                                                 |
| Mesh.Ginja	               	| Ginja mesh data library. Used in SA2B and its ports.                                  |
| ObjectData                  	| Library for handling, reading and writing node and geometry container data.           |
| AnimationData                	| Library for handling, reading and writing animation data.                             |
| Structs                     	| Common structure code between all namespaces.                                         |
| TexName						| Texture name list handlers															|

## Releasing
!! Requires authorization via the X-Hax organisation

1. Edit the version number in src/SA3D.Modeling/SA3D.Modeling.csproj; Example: `<Version>1.0.0</Version>` -> `<Version>2.0.0</Version>`
2. Commit the change but dont yet push.
3. Tag the commit: `git tag -a [version number] HEAD -m "Release version [version number]"`
4. Push with tags: `git push --follow-tags`

This will automatically start the Github `Build and Publish` workflow