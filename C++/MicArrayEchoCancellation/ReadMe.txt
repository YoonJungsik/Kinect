MicArrayEchoCancellation - READ ME 

Copyright (c) Microsoft Corporation. All rights reserved.

=============================
OVERVIEW  
.............................
This sample demonstrates capturing microphone array data using the
Kinect Audio DMO and processing it together with speaker output to
perform Acoustic Echo Cancellation (AEC).
The cleaned audio will be written to a 1 channel wav file.

The microphone array is also used for audio beam angle tracking.

=============================
FILES   
.............................
- MicArrayEchoCancellation.cpp: Defines main function, configures
  Kinect DMO for AEC, captures audio and records WAV file containing
  processed audio.

=============================
OPENING IN VISUAL STUDIO   
.............................
1. Launch Start/All Programs/Microsoft Kinect SDK v1.0/Kinect SDK Sample Browser
   (Start -> typing "Kinect SDK" finds it quickly)
2. In the list of samples, select this sample.
3. Click on "Install" button.
4. Provide a location to install the sample to.
5. Open the Solution file (.sln) that was installed.
