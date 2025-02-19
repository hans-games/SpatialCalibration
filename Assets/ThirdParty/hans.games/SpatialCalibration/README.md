# Offline shared spatial calibration for Meta XR in Unity

Meta XR's Core SDK features [Shared Spatial Anchors](https://developers.meta.com/horizon/documentation/unity/unity-shared-spatial-anchors/) which "allows players who are located in the same physical space to share content while playing the same game." 

However, this only works using Meta's online services. This package provides an offline solution which, although 
requiring a little more setup in each space, should allow for a shared world origin between players.

## Install

1. Setup a Unity Project with the Meta XR All-in-one package
1. Download the [latest release](https://github.com/hans-lv/SpatialCalibration/releases/latest) 
1. Drag into your Unity Project

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
2. After creating the anchors, add the CalibratedOrigin prefab to any scene that needs a shared origin.


