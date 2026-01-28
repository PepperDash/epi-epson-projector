# epi-display-epson_projector
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
        "activeInputs": [
            {
                "key": "SampleString",
                "name": "SampleString"
            }
        ],
        "WarmupTimeMs": 0,
        "CooldownTimeMs": 0
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

- IKeyed
- IHasPowerControlWithFeedback
- IWarmingCooling
- IOnline
- IBasicVideoMuteWithFeedback
- ICommunicationMonitor
- IHasFeedback
- IHasInputs<int>
- IBridgeAdvanced
- IRoutingSinkWithSwitchingWithInputPort
- ISelectableItems<int>
- IQueueMessage
<!-- END Interfaces Implemented -->
<!-- START Base Classes -->
### Base Classes

- JoinMapBaseAdvanced
- EventArgs
- TwoWayDisplayBase
<!-- END Base Classes -->
<!-- START Public Methods -->
### Public Methods

- public void ProcessResponse(string response)
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
- public void StartLensMoveRepeat(ELensFunction func)
- public void StopLensMoveRepeat()
- public void LensFunction(ELensFunction function)
- public void LensPositionRecall(ushort memory)
- public void LinkToApi(Crestron.SimplSharpPro.DeviceSupport.BasicTriList trilist, uint joinStart, string joinMapKey, EiscApiAdvanced bridge)
- public void ProcessResponse(string response)
- public void ProcessResponse(string response)
- public void Select()
- public void Dispatch()
- public void ProcessResponse(string response)
<!-- END Public Methods -->
<!-- START Bool Feedbacks -->
### Bool Feedbacks

- VideoMuteIsOn
- VideoFreezeIsOn
- VideoMuteIsOff
- VideoFreezeIsOff
- IsOnline
<!-- END Bool Feedbacks -->
<!-- START Int Feedbacks -->
### Int Feedbacks

- LampHoursFeedback
- LampHoursFeedback
- CurrentInputValueFeedback
<!-- END Int Feedbacks -->
<!-- START String Feedbacks -->
### String Feedbacks

- SerialNumberFeedback
- SerialNumberFeedback
<!-- END String Feedbacks -->
