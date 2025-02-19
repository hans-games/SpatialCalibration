# Offline shared spatial calibration for Meta XR in Unity

Meta XR's Core SDK features [Shared Spatial Anchors](https://developers.meta.com/horizon/documentation/unity/unity-shared-spatial-anchors/) which "allows players who are located in the same physical space to share content while playing the same game." 

However, this only works using Meta's online services. This package provides an offline solution which, although 
requiring a little more setup in each space, should allow for a shared world origin between players.

## Install

NotImplementedException

## How to use

In short: the user creates [Spatial anchors](https://developers.meta.com/horizon/documentation/unity/unity-spatial-anchors-overview/) 
in the same place in space on each headset. The average position of these anchors can then be used in your application
as a parent object for your scene objects, so that all players see the objects in the same place. Uuids for each anchor 
are saved to local storage, so that they persist between play sessions.

---

*Note:* if you have created Spatial Anchors in the space before you might want to clear history for a clean start. In the 
headset's main menu, go to Privacy & Safety > Device Permissions > Clear physical space history. If you have 'Delete 
Unused Uuids' enabled on the Anchor Manager component, any previously saved Uuids in local storage will also be deleted.

---

1. Open the SpatialCalibrationSampleScene to get an example of how to handle the anchor creation for each headset.
2. 


