# epi-display-epson_projector

4-series Essentials plugin for Epson projector control.

## Configuration

Set plugin options under the device `properties` object.

```json
{
	"type": "epsonProjector",
	"properties": {
		"passKey": "0000",
		"useIpSourceCommands": false,
		"warmingTimeMs": 1000,
		"coolingTimeMs": 2000,
		"activeInputs": [
			{ "key": "Hdmi", "name": "HDMI" },
			{ "key": "DVI", "name": "DVI" },
			{ "key": "Computer", "name": "Computer" },
			{ "key": "Video", "name": "Video" }
		]
	}
}
```

## Notes

- `passKey` is used for ESC/VP.net authentication on IP socket connect.
- `useIpSourceCommands` controls the command set used for `Computer` and `Video` source switching:
	- `false` (default): `SOURCE 11` and `SOURCE 45`
	- `true`: `SOURCE 53` and `SOURCE 56`
- HDMI and DVI source commands are unchanged in both modes.
