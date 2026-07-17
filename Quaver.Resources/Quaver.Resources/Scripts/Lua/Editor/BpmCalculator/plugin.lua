  
-- MoonSharp Documentation - http://www.moonsharp.org/getting_started.html
-- ImGui - https://github.com/ocornut/imgui
-- ImGui.NET - https://github.com/mellinoe/ImGui.NET

-- Main Exeuction Point
function draw()
    draw_menu()
end

function draw_menu()
    imgui.SetNextWindowSize(imgui.CreateVector2(399, 295))
    imgui.Begin("BPM Calculator", imgui_window_flags.NoResize)

    -- State
    local detector = util_getValue("detector", nil)

    local bpm_average = util_getValue("bpm_average", 0)
    local first_tap_audio_time = util_getValue("first_tap_audio_time", 0)
    local tap_count_number = util_getValue("tap_count_number", 0)
    local tap_start = util_getValue("tap_start", 0)

    local bpm = 0
    local offset = 0
    

    if state.CurrentTimingPoint != nil then
        bpm = state.CurrentTimingPoint.Bpm
    end

    if state.CurrentTimingPoint != nil then
        offset = state.CurrentTimingPoint.StartTime
    end

    -- Header
    imgui.TextWrapped("This dialog will allow you calculate the BPM of the song from tapping. After 8 taps, a timing point will be automatically placed.")
    imgui.Dummy(imgui.CreateVector2(0, 5))
    imgui.TextWrapped("Afterwards, adjust the BPM and offset as necessary.")

    imgui.Dummy(imgui.CreateVector2(0, 5))

    imgui.TextWrapped("The longer you tap, the more accurate the calculated BPM will be.")

    imgui.Dummy(imgui.CreateVector2(0, 5))
    
    imgui.TextWrapped("You can also press the 'T' key to tap.")

    imgui.Dummy(imgui.CreateVector2(0, 5))

    -- BPM Textbox
    imgui.TextWrapped("BPM")
    _, bpm_val = imgui.InputFloat("", bpm, 1, 0.1, bpm, 32)

    -- Create a new timing point by changing the BPM
    if state.CurrentTimingPoint == nil and bpm_val != 0 then
        local point = utils.CreateTimingPoint(0, bpm_val)
        actions.PlaceTimingPoint(point)
    end

    -- Changing the existing timing point's BPM
    if state.CurrentTimingPoint != nil and bpm_val != bpm then
        actions.ChangeTimingPointBpm(state.CurrentTimingPoint, bpm_val)
    end

    imgui.Dummy(imgui.CreateVector2(0, 10))

    -- OFfset Textbox
    imgui.TextWrapped("Offset")
    _, offset_val = imgui.InputFloat(" ", offset, 1, 0.1, offset, 32)

    -- Create a new timing point by changing the offset box
    if state.CurrentTimingPoint == nil and offset_val != 0 then
        local point = utils.CreateTimingPoint(offset_val, 0)
        actions.PlaceTimingPoint(point)
    end

    -- Changing the existing timing point's offset
    if state.CurrentTimingPoint != nil and offset_val != offset then
        actions.ChangeTimingPointOffset(state.CurrentTimingPoint, offset_val)
    end

    imgui.Dummy(imgui.CreateVector2(0, 10))

    if imgui.Button("Tap Here") or utils.IsKeyPressed(keys.T) then
        local tap_timer = state.UnixTime

        if tap_count_number == 0 then
            tap_start = tap_timer
            tap_count_number = tap_count_number + 1
            first_tap_audio_time = state.SongTime
        else
            bpm_average = round(60000 * tap_count_number / (tap_timer - tap_start))
            tap_count_number = tap_count_number + 1

            if tap_count_number >= 8 then
                if state.CurrentTimingPoint == nil then
                    local point = utils.CreateTimingPoint(first_tap_audio_time, round(bpm_average))
                    actions.PlaceTimingPoint(point)
                elseif bpm_average != state.CurrentTimingPoint.Bpm then
                    actions.ChangeTimingPointBpm(state.CurrentTimingPoint, bpm_average)

                    if state.CurrentTimingPoint.StartTime != first_tap_audio_time then
                        actions.ChangeTimingPointOffset(state.CurrentTimingPoint, first_tap_audio_time)
                    end
                end
            end
        end
    end

    imgui.SameLine()

    -- Removes the current timing point and resets the state.
    if imgui.Button("Reset") then
        if state.CurrentTimingPoint != nil then
            actions.ResetTimingPoint(state.CurrentTimingPoint)
        end

        bpm_average = 0
        first_tap_audio_time = 0
        tap_count_number = 0
        tap_start = 0
    end    

    -- BPM Detection
    if state.CurrentTimingPoint == nil then
        imgui.SameLine()
        
        local text = "Detect BPM"

        if detector != nil and not detector.Done then
            text = "Please wait..."
        end

        if imgui.Button(text) then
            detector = actions.DetectBpm()
        end
    end

    if detector != nil and detector.Done then
        local point = utils.CreateTimingPoint(detector.SuggestedOffset, detector.HighestConfidenceBpm)
        actions.PlaceTimingPoint(point)

        detector = nil
    end

    state.SetValue("detector", detector);
    state.SetValue("bpm_average", bpm_average)
    state.SetValue("first_tap_audio_time", first_tap_audio_time)
    state.SetValue("tap_count_number", tap_count_number)
    state.SetValue("tap_start", tap_start)

    state.IsWindowHovered = imgui.IsWindowHovered() or imgui.IsItemFocused()

    imgui.End();
end

function handle_taps()

end

function util_getValue(label, defaultValue)
    return state.GetValue(label) or defaultValue
end

function round(n)
    return n % 1 >= 0.5 and math.ceil(n) or math.floor(n)
end