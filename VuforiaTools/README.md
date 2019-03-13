# Unity Vuforia Cloud Image Target tool

This tool enables the user to view and edit Vuforia Cloud Image Targets within Unity.

This tool is intended to replace the current workflow of going to the Vuforia Developer portal to create or modify an Image Target.  It can be dropped into any Unity project with a version of 2018 or greater (though I suspect it can work on older versions as well).  It will create a new menu item in *Window->Vuforia Tools Window*.  

## Setup
Simply drop the folder titled "VuforiaTools" in the Assets folder (required) of your Unity project. You should already have Vuforia installed. If you don't, you may run into problems when creating the configuration file. It's important that the directory is Assets/VuforiaTools.. otherwise you may or may not run into problems.

After this has been added to your Unity project, you can open the Vuforia Tools Window. It will then prompt you to create a configuration file. Press the button, and it will create a VuforiaToolsConfiguration.asset in the Assets>Resources folder, right next to the VuforiaConfiguration.asset. You will need to give it your Access_Key and Secret_Key that you can find for your Vuforia Cloud account. Log into vuforia > Develop > Target Manager > The Cloud Account you want > Database Access Keys. Then you will copy and past the Server Access Keys into the configuration file that was just created. You should be able to hit the refresh button, and it will show you some basic account information and an empty target list.

## Features
Hitting "Refresh" on the Target List will do a few things. It will first do a query to get the list of target_ids associated with that database. It will then loop through all target_ids to get detailed information about each image target, including the name of the image target. It will add them all to a List<TargetSummary> object that's instantiated in your configuration file (this is so that there is persistance between closing and opening Unity). 

After it has gotten detailed information about each image target, it will create/overwrite 3 files in the Assets/Resources/VuforiaToolsData folder. The first file is a json formated text file that holds all the information about each image target, as well as every date you've refreshed and the amount of times that image has been scanned for that day that you've refreshed. The other two files are .csv files that show a table of the total scans each image target has for each time you've hit refresh for that day. It will only store each day once, and if you refresh the whole list multiple times that day, it will overwrite with the most recent refresh. One .csv file is just the transpose of the other to make the data more palatteable. 

If you wish to switch between wide mode and tall mode, that option is a toggle on the configuration file where you pasted in the keys to your account.

Clicking on an item will bring up the rest of the target's settings and allow you to refresh it, modify it, check for similar images in your database, or delete it.

You can scroll through your image targets with the arrow keys.

If you hold down either shift, ctrl, or command while selecting image targets you will select multiple and it will then show them in a list with their current month recos and previous month recos and allow you to do batch activates, deactivates, or deletes on all of the targets you have selected.

After uploading a new target, it will appear at the bottom of the list and the text will probably either be blue or red. Red means that the upload failed, but it is still in the database. You can double check by logging into Vuforia and taking a look. Blue means that the image is still processing, and you can keep pressing refresh on the individual image target until it is no longer blue.

There is a Search Field as well, but that's pretty self explainatory

## Useful Excel Information After Collecting Multiple Days of Data
In the transpose .csv file. If you select the cell B2, it's the first cell that displays how many recos there are. Apply conditional formatting, select enter formula and type =B2>C@, it's important if you click and it shows =$B$2>$C$2 that you remove the $s. Then right click and drag to the second to last cell on the right and apply formatting only. Then with all those cells still highlighted right click and drag down to the last cells at the bottom and apply formatting only. This will essentially highlight each day that any image target was scanned. Useful for knowing which image targets have been used and how recently. 

The date format is YYYYMMDD to make it easy to sort when creating the .csv files

## Limitations
There is no VWS support for getting image thumbnails for your images, so that's not a thing.

There are two functions for getting information about a target with VWS. Retrieving a Target Record, and Retrieving a Target Summary. The Summary has all the information the Record has EXCEPT WIDTH, but it also has some other useful information. I had originally made the Refresh full list button get both the record and the summary, but then it took twice as long, so I removed getting the record entirely. I never use width and always have it set to 1.0, so I didn't bother.

The window is set and not dynamic or adjustable. You can go into the WindowVuforiaTools.cs file and adjust these if you want.

## Acknowledgements and References
A big thank you to CÀ PHÊ KHÔNG ĐƯỜNG (No Sugar Coffee) [for his original post about uploading image targets from within Unity here](https://breakdownblogs.wordpress.com/2015/11/13/adding-image-target-to-cloud-database-use-api-vuforia-and-www-unity/)

If you would like to take a look at the Vuforia Web Services I used to put together the rest of the features for this tool [you can find that here](https://library.vuforia.com/content/vuforia-library/en/articles/Solution/How-To-Use-the-Vuforia-Web-Services-API.html)

If you have any questions, comments, or suggestions feel free to email me at [lweldon@taylordigital.io](mailto:lweldon@taylordigital.io)
