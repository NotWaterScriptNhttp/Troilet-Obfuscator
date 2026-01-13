-- everything
local upval = print
function closure(arg1, ...)
    upval(arg1 .. ": " .. table.concat({...}))
end

closure("Everything", "S", "a", "m", "ple")