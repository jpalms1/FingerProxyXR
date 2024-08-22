Human Anatomy:
Any mesh can be used as long as it has a mesh collider, or any other collider.
The mesh must be set to a layer that is selected as one of the masks that the raycasting 
recognizes. 

trakSTAR:
There should be a ConnectionController object with the Trainer Controller script. 
A trakStar object should have child objects: trakSTAROrigin which is the location of the 
origin of the sensor and have the Trak STAR Controller script attached; finger objects 
and their corresponding proxy objects should also be child objects and each finger object 
should have the Finger Proxy script and the Trak STAR Tool Controller script. 
Within unity in each finger object, set the variable proxy to place to be the proxy object. 
Set the finger mask to the same mask as any finger objects (must be different from the other meshes).
Set the Mask to be the layer of any object that the finger will hit (all of the meshes).

Force and Torque:
All calculations for force and torque and any constants can be found in the FingerProxy script. 

Serial Communication:
The serial comms script should be attached to a stand alone 'Player' object. Within the 
serial comms script, the force and torque are retrived based on the finger object's names. 
The circuit py code must also be open. 

Raycast script:
This script works on the needle, or a similar object to detect what it is hitting.