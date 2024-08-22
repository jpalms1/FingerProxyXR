# Jasmin's ~Official~ 3-DoF Hoxel Control Code for Unity -- Version 1
# Write your code here :-)
import board
import time
import pwmio
from digitalio import DigitalInOut, Direction
from analogio import AnalogIn
import usb_cdc

# Define Presure Input
pres_1 = AnalogIn(board.A8)
pres_2 = AnalogIn(board.A9)
pres_3 = AnalogIn(board.A10)
pres_4 = AnalogIn(board.A11)
pres_5 = AnalogIn(board.A12)
pres_6 = AnalogIn(board.A13)
pres_7 = AnalogIn(board.A14)
pres_8 = AnalogIn(board.A15)

ls_en = board.D40

# Define Solenoid Pins
sol1 = board.A0
sol2 = board.A1
sol3 = board.A2
sol4 = board.A3
sol5 = board.A4
sol6 = board.A5
sol7 = board.A6
sol8 = board.A7

enable_LS = DigitalInOut(ls_en)
enable_LS.direction = Direction.OUTPUT

# Initialize Valves
valve1 = DigitalInOut(sol1)
valve1.direction = Direction.OUTPUT
valve2 = DigitalInOut(sol2)
valve2.direction = Direction.OUTPUT
valve3 = DigitalInOut(sol3)
valve3.direction = Direction.OUTPUT
valve4 = DigitalInOut(sol4)
valve4.direction = Direction.OUTPUT
valve5 = DigitalInOut(sol5)
valve5.direction = Direction.OUTPUT
valve6 = DigitalInOut(sol6)
valve6.direction = Direction.OUTPUT
valve7 = DigitalInOut(sol7)
valve7.direction = Direction.OUTPUT
valve8 = DigitalInOut(sol8)
valve8.direction = Direction.OUTPUT

# Initialize Pumps
pwm_pump1 = pwmio.PWMOut(board.D9, frequency=8000, duty_cycle=0)
pwm_pump2 = pwmio.PWMOut(board.D16, frequency=8000, duty_cycle=0)
pwm_pump3 = pwmio.PWMOut(board.D7, frequency=8000, duty_cycle=0)
pwm_pump4 = pwmio.PWMOut(board.D6, frequency=8000, duty_cycle=0)
pwm_pump5 = pwmio.PWMOut(board.D5, frequency=8000, duty_cycle=0)
pwm_pump6 = pwmio.PWMOut(board.D4, frequency=8000, duty_cycle=0)
pwm_pump7 = pwmio.PWMOut(board.D3, frequency=8000, duty_cycle=0)
pwm_pump8 = pwmio.PWMOut(board.D2, frequency=8000, duty_cycle=0)

# Define timing variables
max_freq = 128
min_period = 1 / max_freq
min_freq = 0.5
max_period = 1 / min_freq
t2 = 0
time_error = 0.05

# Define range of pump speeds
min_pump_speed = 10  # a
max_pump_speed = 100  # b

# Set initial values
min_force = 0.1  # min
max_force = 150.0  # max
X0_prev = min_force
Y0_prev = min_force
Z0_prev = min_force
magF0_prev = 0.0
X1_prev = min_force
Y1_prev = min_force
Z1_prev = min_force
magF1_prev = 0.0


def get_voltage(pin):
    V_ref = 3.3
    resolution = 65535
    return (pin.value * V_ref) / (resolution)


# Outputs pressure in PSI, input is 3.3V scale voltage measured at pressure ADC
def get_pressure(V):
    V_sup = 5
    R1 = 19.6
    R2 = 10
    V_out = V * ((R1 + R2) / R1)

    # P in PSI
    P_min = 0
    P_max = 15

    return 6.89476 * ((V_out - (0.1 * V_sup)) * (P_max - P_min) / (0.8 * V_sup) + P_min)


def duty2bits(duty):
    return int(duty * 65535 / 100)


def pumps_on(d):
    pwm_pump1.duty_cycle = d
    pwm_pump2.duty_cycle = d
    pwm_pump3.duty_cycle = d
    pwm_pump4.duty_cycle = d
    pwm_pump5.duty_cycle = d
    pwm_pump6.duty_cycle = d
    pwm_pump7.duty_cycle = d
    pwm_pump8.duty_cycle = d


# Everything is off and air id released from the line
def exhaustHoxel0():
    pwm_pump1.duty_cycle = 0
    pwm_pump2.duty_cycle = 0
    pwm_pump3.duty_cycle = 0
    pwm_pump4.duty_cycle = 0


def exhaustHoxel1():
    pwm_pump5.duty_cycle = 0
    pwm_pump6.duty_cycle = 0
    pwm_pump7.duty_cycle = 0
    pwm_pump8.duty_cycle = 0


# calculate pump speed depending on commanded force
def get_pump_Speed(force):
    # a = min_pump_speed
    # b = max_pump_speed
    # minVal = 0  # min_force
    # maxVal = 20  # max_force
    # return ((b-a)*(force-minVal) / (maxVal-minVal)) + a

    # pump_speed cannot exceed 100
    force = 15 * abs(force)
    if force >= max_pump_speed:
        pump_Speed = max_pump_speed
    else:
        pump_Speed = force

    return pump_Speed


# ------ Hoxel 0 ------
def x0_pos(d):
    pwm_pump1.duty_cycle = d
    pwm_pump2.duty_cycle = d
    pwm_pump3.duty_cycle = 0
    pwm_pump4.duty_cycle = 0


def x0_neg(d):
    pwm_pump1.duty_cycle = 0
    pwm_pump2.duty_cycle = 0
    pwm_pump3.duty_cycle = d
    pwm_pump4.duty_cycle = d


def y0_pos(d):
    pwm_pump1.duty_cycle = 0
    pwm_pump2.duty_cycle = d
    pwm_pump3.duty_cycle = d
    pwm_pump4.duty_cycle = 0


def y0_neg(d):
    pwm_pump1.duty_cycle = d
    pwm_pump2.duty_cycle = 0
    pwm_pump3.duty_cycle = 0
    pwm_pump4.duty_cycle = d


def z0(d):
    pwm_pump1.duty_cycle = d
    pwm_pump2.duty_cycle = d
    pwm_pump3.duty_cycle = d
    pwm_pump4.duty_cycle = d


# Turn off Hoxel0
def hoxel0Off():
    pwm_pump1.duty_cycle = 0
    pwm_pump2.duty_cycle = 0
    pwm_pump3.duty_cycle = 0
    pwm_pump4.duty_cycle = 0


# ------ Hoxel 1 ------


def x1_pos(d):
    pwm_pump5.duty_cycle = 0
    pwm_pump6.duty_cycle = 0
    pwm_pump7.duty_cycle = d
    pwm_pump8.duty_cycle = d


def x1_neg(d):
    pwm_pump5.duty_cycle = d
    pwm_pump6.duty_cycle = d
    pwm_pump7.duty_cycle = 0
    pwm_pump8.duty_cycle = 0


def y1_pos(d):
    pwm_pump5.duty_cycle = 0
    pwm_pump6.duty_cycle = d
    pwm_pump7.duty_cycle = d
    pwm_pump8.duty_cycle = 0


def y1_neg(d):
    pwm_pump5.duty_cycle = d
    pwm_pump6.duty_cycle = 0
    pwm_pump7.duty_cycle = 0
    pwm_pump8.duty_cycle = d


def z1(d):
    pwm_pump5.duty_cycle = d
    pwm_pump6.duty_cycle = d
    pwm_pump7.duty_cycle = d
    pwm_pump8.duty_cycle = d


# Turn off Hoxel1
def hoxel1Off():
    pwm_pump5.duty_cycle = 0
    pwm_pump6.duty_cycle = 0
    pwm_pump7.duty_cycle = 0
    pwm_pump8.duty_cycle = 0


# ------ Actuation ------
# ------ 3-DoF ------
kZ = 1.0
scale = 4.0


def moveHoxel0(X0, Y0, Z0, magF0, shear0, M0):
    if magF0 <= min_force:
        exhaustHoxel0()
    else:
        if abs(Z0) >= scale * shear0:  # Normal force only
            pwm_pump1.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
            pwm_pump2.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
            pwm_pump3.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
            pwm_pump4.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
        else:
            if abs(X0) >= scale * abs(Y0):  # X is dominant -> Move X
                if X0 >= 0.0:
                    x0_pos(duty2bits(get_pump_Speed(X0)))
                else:
                    x0_neg(duty2bits(get_pump_Speed(X0)))
            elif abs(Y0) >= scale * abs(X0):  # Y is dominant -> Move Y
                if Y0 >= 0.0:
                    y0_pos(duty2bits(get_pump_Speed(Y0)))
                else:
                    y0_neg(duty2bits(get_pump_Speed(Y0)))
            else:
                if X0 >= 0.0 and Y0 >= 0.0:
                    pwm_pump1.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump2.duty_cycle = duty2bits(get_pump_Speed(magF0))
                    +duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump3.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump4.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                elif X0 >= 0.0 and Y0 < 0.0:
                    pwm_pump1.duty_cycle = duty2bits(get_pump_Speed(magF0))
                    +duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump2.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump3.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump4.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                elif X0 < 0.0 and Y0 >= 0.0:
                    pwm_pump1.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump2.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump3.duty_cycle = duty2bits(get_pump_Speed(magF0))
                    +duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump4.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                else:
                    pwm_pump1.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump2.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump3.duty_cycle = duty2bits(kZ * get_pump_Speed(Z0))
                    pwm_pump4.duty_cycle = duty2bits(get_pump_Speed(magF0))
                    +duty2bits(kZ * get_pump_Speed(Z0))


def moveHoxel1(X1, Y1, Z1, magF1, shear1):
    if magF1 <= min_force:
        exhaustHoxel1()
    else:
        if abs(Z1) >= scale * shear1:  # Normal force only
            pwm_pump5.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
            pwm_pump6.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
            pwm_pump7.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
            pwm_pump8.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
        else:
            if abs(X1) >= scale * abs(Y1):  # X is dominant -> Move X
                if X1 >= 0.0:
                    x1_pos(duty2bits(get_pump_Speed(X1)))
                else:
                    x1_neg(duty2bits(get_pump_Speed(X1)))
            elif abs(Y1) >= scale * abs(X1):  # Y is dominant -> Move Y
                if Y1 >= 0.0:
                    y1_pos(duty2bits(get_pump_Speed(Y1)))
                else:
                    y1_neg(duty2bits(get_pump_Speed(Y1)))
            else:
                if X1 >= 0.0 and Y1 >= 0.0:
                    pwm_pump5.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump6.duty_cycle = duty2bits(get_pump_Speed(magF1))
                    +duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump7.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump8.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                elif X1 >= 0.0 and Y1 < 0.0:
                    pwm_pump5.duty_cycle = duty2bits(get_pump_Speed(magF1))
                    +duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump6.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump7.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump8.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                elif X1 < 0.0 and Y1 >= 0.0:
                    pwm_pump5.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump6.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump7.duty_cycle = duty2bits(get_pump_Speed(magF1))
                    +duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump8.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                else:
                    pwm_pump5.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump6.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump7.duty_cycle = duty2bits(kZ * get_pump_Speed(Z1))
                    pwm_pump8.duty_cycle = duty2bits(get_pump_Speed(magF1))
                    +duty2bits(kZ * get_pump_Speed(Z1))


enable_LS.value = False
enable_LS.value = True
# Everything off
exhaustHoxel0()
exhaustHoxel1()

# Set initial frequency
i = min_freq
t = 0.3
t2 = 0.3
# valve0c.value = False
# valve1c.value = False
time.sleep(3)
# Start vacuum
# valve0c.value = True
# valve1c.value = True

# SERIAL: ~~~~~~~~~~~~~~~~~~~~~~~~

# main
buffer = ""
serial = usb_cdc.console


def read_serial(serial):
    text = ""
    available = serial.in_waiting
    while available:
        raw = serial.readline(available)
        text = raw.decode("utf-8")
        available = serial.in_waiting
    return text


while True:
    # buffer += read_serial(serial)
    # print("running")
    if serial.in_waiting > 0:
        # print("in the loop")
        # buffer += read_serial(serial)
        data = serial.readline(serial.in_waiting)
        data = data.decode('uft-8')
        # strip line end
        # data = buffer[:-1]
        print(data)
        # clear buffer
        # buffer = ""
        # handle input
        data_list = data.split(" ")
        # Set current values for each device direction
        X0 = float(data_list[0])
        Y0 = float(data_list[1])
        Z0 = float(data_list[2])
        magF0 = float(data_list[3])
        shear0 = float(data_list[4])
        M0 = float(data_list[5])
        # X1 = float(data_list[5])
        # Y1 = float(data_list[6])
        # Z1 = float(data_list[7])
        # magF1 = float(data_list[8])
        # shear1 = float(data_list[9])
        #         X0 = abs(X0)
        #         Y0 = abs(Y0)
        #         Z0 = abs(Z0)
        #         magF0 = abs(magF0)
        #         shear0 = abs(shear0)

        # Hoxel 0:
        moveHoxel0(X0, Y0, Z0, magF0, shear0, M0)
        # Hoxel 1:
        # moveHoxel1(X1, Y1, Z1, magF1, shear1)

        time.sleep(0.001)
    else:
        time.sleep(0.001)
