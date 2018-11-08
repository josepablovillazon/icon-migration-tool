# icon-migration-tool
Tool to convert icons from SVG and embed to the XML action

This project allows to load SVG icons into the XML file action.

SVG icons, need to be named same as the action handler.
PLAY.XML ---> play.svg

Te tool will read the play.svg source and convert to BMP base64 format and replace inside the XML <Icon> tag.
