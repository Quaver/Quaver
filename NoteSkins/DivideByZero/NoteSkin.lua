--Unlimited Stepman Works Noteskin.lua for SM 5.0.12

--I am the bone of my noteskin
--Arrows are my body, and explosions are my blood
--I have created over a thousand noteskins
--Unknown to death
--Nor known to life
--Have withstood pain to create many noteskins
--Yet these hands will never hold anything
--So as I pray, Unlimited Stepman Works

local USWN = {};

--Defining on which direction the other directions should be bassed on
--This will let us use less files which is quite handy to keep the noteskin directory nice
--Do remember this will Redirect all the files of that Direction to the Direction its pointed to
--If you only want some files to be redirected take a look at the "custom hold/roll per direction"
USWN.ButtonRedir =
{
	Up = "Down",
	Down = "Down",
	Left = "Down",
	Right = "Down",
};

-- Defined the parts to be rotated at which degree
USWN.Rotate =
{
	Up = 180,
	Down = 0,
	Left = 90,
	Right = -90,
};


--Define elements that need to be redirected
USWN.ElementRedir =
{
	["Tap Fake"] = "Tap Note",
	["Roll Explosion"] = "Hold Explosion",
	["Hold Body Inactive"] = "Hold Body Active",
	["Hold Bottomcap Inactive"] = "Hold Bottomcap Active",
	["Roll Body Inactive"] = "Roll Body Active",
	["Roll Bottomcap Inactive"] = "Roll Bottomcap Active",
};

-- Parts of noteskins which we want to rotate
USWN.PartsToRotate =
{
	["Receptor"] = true,
	["Tap Note"] = true,
	["Tap Fake"] = true,
	["Tap Addition"] = true,
	["Hold Explosion"] = true,
	["Hold Head Active"] = true,
	["Hold Head Inactive"] = true,
	["Roll Explosion"] = true,
	["Roll Head Active"] = true,
	["Roll Head Inactive"] = true,
};

-- Parts that should be Redirected to _Blank.png
-- you can add/remove stuff if you want
USWN.Blank =
{
	["Tap Explosion Bright"] = true,
	["Tap Explosion Dim"] = true,
	["Hold Topcap Active"] = true,
	["Hold Topcap Inactive"] = true,
	["Roll Topcap Active"] = true,
	["Roll Topcap Inactive"] = true,
	["Hold Tail Active"] = true,
	["Hold Tail Inactive"] = true,
	["Roll Tail Active"] = true,
	["Roll Tail Inactive"] = true,
};

-- < 
--Between here we usally put all the commands the noteskin.lua needs to do, some are extern in other files
--If you need help with lua go to http://dguzek.github.io/Lua-For-SM5/API/Lua.xml there are a bunch of codes there
--Also check out common it has a load of lua codes in files there
--Just play a bit with lua its not that hard if you understand coding
--But SM can be an ass in some cases, and some codes jut wont work if you dont have the noteskin on FallbackNoteSkin=common in the metric.ini 
function USWN.Load()
	local sButton = Var "Button";
	local sElement = Var "Element";

	local Button = USWN.ButtonRedir[sButton] or sButton;	
			
	--Setting global element
	local Element = USWN.ElementRedir[sElement] or sElement;

	if string.find(sElement, "Head") then
		Element = "Hold Head";
	end
	
	--Returning first part of the code, The redirects, Second part is for commands
	local t = LoadActor(NOTESKIN:GetPath(Button,Element));
	
	--Set blank redirects
	if USWN.Blank[sElement] then
		t = Def.Actor {};
		--Check if element is sprite only
		if Var "SpriteOnly" then
			t = LoadActor(NOTESKIN:GetPath("","_blank"));
		end
	end
	
	if USWN.PartsToRotate[sElement] then
		t.BaseRotationZ = USWN.Rotate[sButton] or nil;
	end
	
	--Explosion should not be rotated, It calls other actors
	if sElement == "Explosion" then
		t.BaseRotationZ = nil;
	end
		
	return t;
end
-- >

-- dont forget to return cuz else it wont work ;>
return USWN;
