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
<!-- START Minimum Essentials Framework Versions -->
### Minimum Essentials Framework Versions

- 2.4.7
<!-- END Minimum Essentials Framework Versions -->
<!-- START Config Example -->
### Config Example

```json
{
    "key": "GeneratedKey",
    "uid": 1,
    "name": "GeneratedName",
    "type": "epsonProjector",
    "group": "Group",
    "properties": {
        "Control": "SampleValue",
        "Monitor": "SampleValue",
        "EnableBridgeComms": true,
        "passKey": "SampleString",
        "useIpSourceCommands": true,
        "coolingTimeMs": "SampleValue",
        "warmingTimeMs": "SampleValue",
        "activeInputs": [
            {
                "key": "SampleString",
                "name": "SampleString"
            }
        ]
    }
}
```
<!-- END Config Example -->
<!-- START Supported Types -->
### Supported Types

- epsonProjector
<!-- END Supported Types -->
<!-- START Join Maps -->
### Join Maps

#### Digitals

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Power Off |
| 2 | R | Power On |
| 9 | R | Warming |
| 10 | R | Cooling |
| 21 | R | Mute Off |
| 22 | R | Mute On |
| 23 | R | Mute Toggle |
| 5 | R | Mute Off Legacy |
| 6 | R | Mute On Legacy |
| 7 | R | Mute Toggle Legacy |
| 3 | R | Is Projector |
| 50 | R | Is Online |
| 11 | R | Input Select |
| 29 | R | Freeze Off |
| 30 | R | Freeze On |
| 37 | R | Freeze Toggle |
| 40 | R | VShiftPlus |
| 41 | R | VShiftMinus |
| 42 | R | HShiftPlus |
| 43 | R | HShiftMinus |
| 44 | R | FocusPlus |
| 45 | R | FocusMinus |
| 46 | R | ZoomPlus |
| 47 | R | ZoomMinus |

#### Analogs

| Join | Type (RW) | Description |
| --- | --- | --- |
| 2 | R | Lamp Hours |
| 5 | R | Lens Position |

#### Serials

| Join | Type (RW) | Description |
| --- | --- | --- |
| 1 | R | Name |
| 2 | R | Status |
| 3 | R | Serial Number |
<!-- END Join Maps -->
<!-- START Interfaces Implemented -->
### Interfaces Implemented

- IHasPowerControlWithFeedback
- IWarmingCooling
- IOnline
- IBasicVideoMuteWithFeedback
- ICommunicationMonitor
- IHasFeedback
- IHasInputs<int>
- IKeyed
- ISelectableItems<int>
- IQueueMessage
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- EventArgs
- TwoWayDisplayBase
- JoinMapBaseAdvanced
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void LinkToApi(BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
- public void SetHDMI()
- public void SetDVI()
- public void SetComputer()
- public void SetVideo()
- public void VideoMuteToggle()
- public void VideoMuteOn()
- public void VideoMuteOff()
- public void VideoFreezeToggle()
- public void VideoFreezeOn()
- public void VideoFreezeOff()
- public void StartLensMoveRepeat(eLensFunction func)
- public void StopLensMoveRepeat()
- public void LensFunction(eLensFunction function)
- public void LensPositionRecall(ushort memory)
- public void ProcessResponse(string response)
- public void ProcessResponse(string response)
- public void Select()
- public void Dispatch()
- public void ProcessResponse(string response)
- public void ProcessResponse(string response)
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- IpChangeFeedback
- VideoMuteIsOn
- VideoFreezeIsOn
- PowerIsOnFeedback
- PowerIsOffFeedback
- VideoMuteIsOff
- VideoFreezeIsOff
- IsOnline
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- LampHoursFeedback
- CurrentInputValueFeedback
- LampHoursFeedback
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->
### String Feedbacks

- SerialNumberFeedback
- SerialNumberFeedback
<!-- END String Feedbacks -->
