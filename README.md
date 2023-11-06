# SA3D.Modeling
A Sonic Adventure modeling library with support for all game related model formats. Also contains support for various other SEGA based games, although support is not guaranteed.

## Contents
| Namespace (SA3D.Modeling.*) 	| Description                                                                                                                          	|
|-----------------------------	|--------------------------------------------------------------------------------------------------------------------------------------	|
| File                        	| Model data storage file handlers for select native- and X-Hax custom file-formats.                                                   	|
| Mesh                        	| Library for handling, reading and writing mesh data.                                                                                 	|
| Mesh.Basic                  	| Basic mesh data library. Used in SA1 (everything) and SA2 (collision geometry only)                                                  	|
| Mesh.Chunk                  	| Chunk mesh data library. Used in SA2.                                                                                                	|
| Mesh.Gamecube               	| Gamecube-like mesh data library. Used in SA2B and its ports.                                                                         	|
| Mesh.Buffer                 	| SA3D internal mesh format. Used for conversion and rendering purposes. Is a simplified version of Chunk and mixes in Basic elements. 	|
| Mesh.Weighted               	| SA3D internal mesh format. Used only for conversion purposes, as it is more in line with most modern mesh formats.                   	|
| ObjectData                  	| Library for handling, reading and writing node and geometry container data.                                                          	|
| Animation                   	| Library for handling, reading and writing animation data.                                                                            	|
| Structs                     	| Common structure code between all namespaces.                                                                                        	|
| Strippify                   	| Triangle strip generating and handling code.                                                                                         	|

## Releasing
!! Requires authorization via the X-Hax organisation

1. Edit the version number in src/SA3D.Common/SA3D.Common.csproj; Example: `<Version>1.0.0</Version>` -> `<Version>2.0.0</Version>`
2. Commit the change but dont yet push.
3. Tag the commit: `git tag -a [version number] HEAD -m "Release version [version number]"`
4. Push with tags: `git push --follow-tags`

This will automatically start the Github `Build and Publish` workflow