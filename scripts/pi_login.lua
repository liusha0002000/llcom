--树莓派登陆脚本，需开启树莓派串口才能使用

local USERNAME = "pi" --这里修改为你的树莓派用户名
local PASSWORD = "raspiberry" ----这里修改为你的树莓派密码

local UART_MSG_ID = "PI-UART"

--等待列表，格式：["等待字符串"] = "收到后要publish的消息"
local subList = {}

--串口接收回调，分配消息
uartReceive = function (str)
    log.info("uart receive",str)
    for i,j in pairs(subList) do
		if str:find(i) then
			sys.publish(j,str)
			subList[i] = nil
		end
    end
end

local waitId = 0
--根据串口接收来发送的函数
function uartWaitSend(waitString,sendString,timeoutMs)
	waitId = waitId + 1
	--唯一的消息等待id
    subList[waitString] = "uartWait"..waitId
	return sys.waitUntil(subList[waitString],timeoutMs)
end

--登陆任务
sys.taskInit(function()
    local r,s = uartWaitSend("login",USERNAME.."\r\n",2000)
    r,s = uartWaitSend("Password",PASSWORD.."\r\n",2000)
end)

