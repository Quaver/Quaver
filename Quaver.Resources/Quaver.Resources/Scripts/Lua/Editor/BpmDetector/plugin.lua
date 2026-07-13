  
-- MoonSharp Documentation - http://www.moonsharp.org/getting_started.html
-- ImGui - https://github.com/ocornut/imgui
-- ImGui.NET - https://github.com/mellinoe/ImGui.NET

-- Main Exeuction Point
function draw()
    draw_menu()
end

function draw_menu()
    local detector = util_getValue("detector", nil)

    imgui.SetNextWindowSize(imgui.CreateVector2(398, 168))
    imgui.Begin("BPM Detector", imgui_window_flags.NoResize)

    imgui.TextWrapped("This dialog will allow you to automatically detect the BPM and offset of the song.")
    
    imgui.Dummy(imgui.CreateVector2(0, 5))

    imgui.TextWrapped("Keep in mind that this is not always accurate, and you may need to do further adjustments.")

    imgui.Dummy(imgui.CreateVector2(0, 5))

    if detector == nil then
        if imgui.Button("Detect") then
            detector = actions.DetectBpm()
        end
    end

    if detector != nil then
        if detector.Done then
            -- Find BPM with the highest confidence
            imgui.TextWrapped("Found " .. detector.HighestConfidenceBpm .. " BPM" .. " with " .. detector.HighestConfidenceBpmPercentage .. "%% confidence.")    
            imgui.Dummy(imgui.CreateVector2(0, 5))      
            
            if imgui.Button("Apply BPM at " .. detector.SuggestedOffset .. "ms offset") then
                local point = utils.CreateTimingPoint(detector.SuggestedOffset, detector.HighestConfidenceBpm)
                actions.PlaceTimingPoint(point)
            end
        
            imgui.SameLine()

            if imgui.Button("Apply BPM At Current Time") then
                local point = utils.CreateTimingPoint(state.SongTime, detector.HighestConfidenceBpm)
                actions.PlaceTimingPoint(point)
            end
        
            imgui.SameLine()

            if imgui.Button("Reset") then
                detector = nil
            end
        else
            imgui.TextWrapped("Detecting BPM! Please wait...")
        end
    end

    state.IsWindowHovered = imgui.IsWindowHovered() or imgui.IsItemFocused()
    imgui.End();

    state.SetValue("detector", detector);
end

function util_getValue(label, defaultValue)
    return state.GetValue(label) or defaultValue
end