// Licensed under CC BY-NC 4.0: free to use and modify with attribution, non-commercial use only.

using System.IO.Ports;
using System.Text.RegularExpressions;
using System.Management;
using System.Reflection;

namespace ODrive_GUI
{
    public partial class Form1 : Form
    {
        private Panel ScrollablePanel;
        private TableLayoutPanel TableLayoutPanel;

        private ComboBox PortSelection;
        public SerialPort SerialPort1;
        private Button RefreshButton;
        private Button ConnectButton;
        private Button DisconnectButton;
        private Button EraseConfigurationButton;

        private int FirmwareVersion;

        private Dictionary<string, long> MotorTypes = new Dictionary<string, long>
        {
            {"MOTOR_TYPE_HIGH_CURRENT", 0},
            {"MOTOR_TYPE_GIMBAL", 2},
            {"MOTOR_TYPE_ACIM", 3}
        };

        private Dictionary<string, long> AxisStates = new Dictionary<string, long>
        {
            {"UNDEFINED", 0},
            {"IDLE", 1},
            {"STARTUP_SEQUENCE", 2},
            {"FULL_CALIBRATION_SEQUENCE", 3},
            {"MOTOR_CALIBRATION", 4},
            {"ENCODER_INDEX_SEARCH", 6},
            {"ENCODER_OFFSET_CALIBRATION", 7},
            {"CLOSED_LOOP_CONTROL", 8},
            {"LOCKIN_SPIN", 9},
            {"INPUT_TORQUE_CONTROL", 10},
            {"INPUT_VOLTAGE_CONTROL", 11},
            {"CURRENT_CONTROL", 12},
            {"VELOCITY_CONTROL", 13},
            {"POSITION_CONTROL", 14},
            {"TRAJECTORY_CONTROL", 15},
            {"DISCOVERING_USB", 16},
            {"UNKNOWN", 255}
        };

        private Dictionary<string, long> EncoderModes = new Dictionary<string, long>
        {
            {"ENCODER_MODE_INCREMENTAL", 0},
            {"ENCODER_MODE_HALL", 1},
            {"ENCODER_MODE_SINCOS", 2},
            {"ENCODER_MODE_SPI_ABS_CUI", 256},
            {"ENCODER_MODE_SPI_ABS_AMS", 257},
            {"ENCODER_MODE_SPI_ABS_AEAT", 258},
            {"ENCODER_MODE_SPI_ABS_AEAT_MARK", 259},
            {"ENCODER_MODE_SPI_ABS_CUI_2048", 260},
            {"ENCODER_MODE_SPI_ABS_CUI_4096", 261},
            {"ENCODER_MODE_SPI_ABS_CUI_8192", 262},
            {"ENCODER_MODE_SPI_ABS_AMS_512", 263},
            {"ENCODER_MODE_SPI_ABS_AMS_1024", 264},
            {"ENCODER_MODE_SPI_ABS_AMS_2048", 265},
            {"ENCODER_MODE_SPI_ABS_AMS_4096", 266},
            {"ENCODER_MODE_SPI_ABS_AMS_8192", 267},
            {"ENCODER_MODE_SPI_ABS_AMS_16384", 268},
            {"ENCODER_MODE_SPI_ABS_AMS_32768", 269},
            {"ENCODER_MODE_SPI_ABS_AMS_65536", 270},
            {"ENCODER_MODE_SPI_ABS_AMS_131072", 271},
            {"ENCODER_MODE_SPI_ABS_AMS_262144", 272},
            {"ENCODER_MODE_SPI_ABS_AMS_524288", 273},
            {"ENCODER_MODE_SPI_ABS_AMS_1048576", 274},
            {"ENCODER_MODE_SPI_ABS_AMS_2097152", 275},
            {"ENCODER_MODE_SPI_ABS_AMS_4194304", 276},
            {"ENCODER_MODE_SPI_ABS_AMS_8388608", 277},
            {"ENCODER_MODE_SPI_ABS_AMS_16777216", 278},
            {"ENCODER_MODE_SPI_ABS_AMS_33554432", 279},
            {"ENCODER_MODE_SPI_ABS_AMS_67108864", 280},
            {"ENCODER_MODE_SPI_ABS_AMS_134217728", 281},
            {"ENCODER_MODE_SPI_ABS_AMS_268435456", 282},
            {"ENCODER_MODE_SPI_ABS_AMS_MASK", 4294901760}
        };

        private Dictionary<string, long> ControlModes = new Dictionary<string, long>
        {
            {"CTRL_MODE_VOLTAGE_CONTROL", 0},
            {"CTRL_MODE_CURRENT_CONTROL", 1},
            {"CTRL_MODE_VELOCITY_CONTROL", 2},
            {"CTRL_MODE_POSITION_CONTROL", 3},
            {"CTRL_MODE_TRAJECTORY_CONTROL", 4}
        };

        private Dictionary<string, long> InputModes = new Dictionary<string, long>
        {
            {"INPUT_MODE_INACTIVE", 0},
            {"INPUT_MODE_PASSTHROUGH", 1},
            {"INPUT_MODE_VEL_RAMP", 2},
            {"INPUT_MODE_POS_FILTER", 3}
        };

        private Dictionary<string, long> Bools = new Dictionary<string, long>
        {
            {"False", 0},
            {"True", 1}
        };

        private Dictionary<string, long> BoardErrors = new Dictionary<string, long>
        {
            {"NONE", 0},
            {"CONTROL_ITERATION_MISSED", 1},
            {"DC_BUS_UNDER_VOLTAGE", 2},
            {"DC_BUS_OVER_VOLTAGE", 4},
            {"DC_BUS_OVER_REGEN_CURRENT", 8},
            {"DC_BUS_OVER_CURRENT", 16},
            {"BRAKE_DEADTIME_VIOLATION", 32},
            {"BRAKE_DUTY_CYCLE_NAN", 64},
            {"INVALID_BRAKE_RESISTANCE", 128}
        };

        private Dictionary<string, long> AxisErrors = new Dictionary<string, long>
        {
            {"NONE", 0},
            {"INVALID_STATE", 1},
            {"WATCHDOG_TIMER_EXPIRED", 2048},
            {"MIN_ENDSTOP_PRESSED", 4096},
            {"MAX_ENDSTOP_PRESSED", 8192},
            {"ESTOP_REQUESTED", 16384},
            {"HOMING_WITHOUT_ENDSTOP", 131072},
            {"OVER_TEMP", 262144},
            {"UNKNOWN_POSITION", 524288}
        };

        private Dictionary<string, long> MotorErrors = new Dictionary<string, long>
        {
            {"NONE", 0},
            {"PHASE_RESISTANCE_OUT_OF_RANGE", 1},
            {"PHASE_INDUCTANCE_OUT_OF_RANGE", 2},
            {"DRV_FAULT", 8},
            {"CONTROL_DEADLINE_MISSED", 16},
            {"MODULATION_MAGNITUDE", 128},
            {"CURRENT_SENSE_SATURATION", 1024},
            {"CURRENT_LIMIT_VIOLATION", 4096},
            {"MODULATION_IS_NAN", 65536},
            {"MOTOR_THERMISTOR_OVER_TEMP", 131072},
            {"FET_THERMISTOR_OVER_TEMP", 262144},
            {"TIMER_UPDATE_MISSED", 524288},
            {"CURRENT_MEASUREMENT_UNAVAILABLE", 1048576},
            {"CONTROLLER_FAILED", 2097152},
            {"I_BUS_OUT_OF_RANGE", 4194304},
            {"BRAKE_RESISTOR_DISARMED", 8388608},
            {"SYSTEM_LEVEL", 16777216},
            {"BAD_TIMING", 33554432},
            {"UNKNOWN_PHASE_ESTIMATE", 67108864},
            {"UNKNOWN_PHASE_VEL", 134217728},
            {"UNKNOWN_TORQUE", 268435456},
            {"UNKNOWN_CURRENT_COMMAND", 536870912},
            {"UNKNOWN_CURRENT_MEASUREMENT", 1073741824},
            {"UNKNOWN_VBUS_VOLTAGE", 2147483648},
            {"UNKNOWN_VOLTAGE_COMMAND", 4294967296},
            {"UNKNOWN_GAINS", 8589934592},
            {"CONTROLLER_INITIALIZING", 17179869184},
            {"UNBALANCED_PHASES", 34359738368}
        };

        private Dictionary<string, long> EncoderErrors = new Dictionary<string, long>
        {
            {"NONE", 0},
            {"UNSTABLE_GAIN", 1},
            {"CPR_POLEPAIRS_MISMATCH", 2},
            {"NO_RESPONSE", 4},
            {"UNSUPPORTED_ENCODER_MODE", 8},
            {"ILLEGAL_HALL_STATE", 16},
            {"INDEX_NOT_FOUND_YET", 32},
            {"ABS_SPI_TIMEOUT", 64},
            {"ABS_SPI_COM_FAIL", 128},
            {"ABS_SPI_NOT_READY", 256},
            {"HALL_NOT_CALIBRATED_YET", 512}
        };

        private Dictionary<string, long> ControllerErrors = new Dictionary<string, long>
        {
            {"NONE", 0},
            {"OVERSPEED", 1},
            {"INVALID_INPUT_MODE", 2},
            {"UNSTABLE_GAIN", 4},
            {"INVALID_MIRROR_AXIS", 8},
            {"INVALID_LOAD_ENCODER", 16},
            {"INVALID_ESTIMATE", 32},
            {"INVALID_CIRCULAR_RANGE", 64},
            {"SPINOUT_DETECTED", 128}
        };

        private int CurrentRow = 0;

        private List<Dictionary<string, object>> DisplayObjects = new List<Dictionary<string, object>>();

        private List<Dictionary<string, object>> ErrorDisplayObjects = new List<Dictionary<string, object>>();

        public Form1()
        {
            InitializeComponent();
            SetControlFont(this, new Font("Consolas", 10));
        }

        private void SetControlFont(Control control, Font font)
        {
            control.Font = font;
            foreach (Control childControl in control.Controls)
            {
                SetControlFont(childControl, font);
            }
        }

        private void InitializeComponent()
        {
            Text = "ODrive V3.X GUI";
            Size = new Size(1900, 800);
            MinimumSize = new Size(350, 200);
            Icon = new Icon(Assembly.GetExecutingAssembly().GetManifestResourceStream("ODrive_GUI.odrive white.ico"));

            ScrollablePanel = new Panel();
            TableLayoutPanel = new TableLayoutPanel();

            PortSelection = new ComboBox();
            RefreshButton = new Button();
            ConnectButton = new Button();
            DisconnectButton = new Button();
            EraseConfigurationButton = new Button();
            SerialPort1 = new SerialPort();

            DisconnectButton.Enabled = false;
            PortSelection.DropDownStyle = ComboBoxStyle.DropDownList;
            Refresh_Click(null, null);

            RefreshButton.Text = "Refresh";
            ConnectButton.Text = "Connect";
            DisconnectButton.Text = "Disconnect";
            EraseConfigurationButton.Text = "Erase configuration";

            ScrollablePanel.Dock = DockStyle.Fill;
            ScrollablePanel.AutoScroll = true;
            Controls.Add(ScrollablePanel);
            ScrollablePanel.Controls.Add(TableLayoutPanel);

            TableLayoutPanel.ColumnCount = 10;
            TableLayoutPanel.RowCount = 10;
            TableLayoutPanel.AutoSize = true;
            TableLayoutPanel.AutoSizeMode = AutoSizeMode.GrowAndShrink;

            for (int i = 0; i < TableLayoutPanel.ColumnCount; i++)
                TableLayoutPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 185F));

            for (int i = 0; i < TableLayoutPanel.RowCount; i++)
                TableLayoutPanel.RowStyles.Add(i < 2 ? new RowStyle(SizeType.Absolute, 30F) : new RowStyle(SizeType.AutoSize));

            HeaderObject headerObject = new HeaderObject(TableLayoutPanel, 0, 0, 10);

            PortSelection.Dock = DockStyle.Fill;
            TableLayoutPanel.Controls.Add(PortSelection, 0, 1);
            TableLayoutPanel.SetColumnSpan(PortSelection, 4);

            RefreshButton.Dock = DockStyle.Fill;
            TableLayoutPanel.Controls.Add(RefreshButton, 2, 1);
            TableLayoutPanel.SetColumnSpan(RefreshButton, 2);

            ConnectButton.Dock = DockStyle.Fill;
            TableLayoutPanel.Controls.Add(ConnectButton, 3, 1);
            TableLayoutPanel.SetColumnSpan(ConnectButton, 2);

            DisconnectButton.Dock = DockStyle.Fill;
            TableLayoutPanel.Controls.Add(DisconnectButton, 4, 1);
            TableLayoutPanel.SetColumnSpan(DisconnectButton, 2);

            EraseConfigurationButton.Dock = DockStyle.Fill;
            TableLayoutPanel.Controls.Add(EraseConfigurationButton, 0, 2);
            TableLayoutPanel.SetColumnSpan(EraseConfigurationButton, 10);

            RefreshButton.Click += new EventHandler(Refresh_Click);
            ConnectButton.Click += new EventHandler(Connect_Click);
            DisconnectButton.Click += new EventHandler(Disconnect_Click);
            EraseConfigurationButton.Click += new EventHandler(EraseConfiguration_Click);

            // Axis container group boxes
            GroupBox Axis0 = CreateGroupBox("Axis 0", TableLayoutPanel, 0, 3);
            TableLayoutPanel.SetColumnSpan(Axis0, 5);
            TableLayoutPanel Axis0Layout = (TableLayoutPanel)Axis0.Controls[0];

            GroupBox Axis1 = CreateGroupBox("Axis 1", TableLayoutPanel, 5, 3);
            TableLayoutPanel.SetColumnSpan(Axis1, 5);
            TableLayoutPanel Axis1Layout = (TableLayoutPanel)Axis1.Controls[0];

            // Add group boxes to Axis 0 container
            Axis0Layout.Controls.Add(CreateGroupBox("Essential Settings", Axis0Layout, 0, 0));
            EssentialSettings((GroupBox)Axis0Layout.Controls[0], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("Test Settings", Axis0Layout, 0, 1));
            TestSettings((GroupBox)Axis0Layout.Controls[1], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("Errors", Axis0Layout, 0, 2));
            Errors((GroupBox)Axis0Layout.Controls[2], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("Troubleshooting", Axis0Layout, 0, 3));
            TroubleShooting((GroupBox)Axis0Layout.Controls[3], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("Start-up Sequence", Axis0Layout, 0, 4));
            StartUpSequence((GroupBox)Axis0Layout.Controls[4], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("Additional Settings", Axis0Layout, 0, 5));
            AdditionalSettings((GroupBox)Axis0Layout.Controls[5], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("Application Setup", Axis0Layout, 0, 6));
            ApplicationSetup((GroupBox)Axis0Layout.Controls[6], "axis0");

            Axis0Layout.Controls.Add(CreateGroupBox("PID Tuning", Axis0Layout, 0, 7));
            PIDTuning((GroupBox)Axis0Layout.Controls[7], "axis0");

            // Add group boxes to Axis 1 container
            Axis1Layout.Controls.Add(CreateGroupBox("Essential Settings", Axis1Layout, 0, 0));
            EssentialSettings((GroupBox)Axis1Layout.Controls[0], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("Test Settings", Axis1Layout, 0, 1));
            TestSettings((GroupBox)Axis1Layout.Controls[1], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("Errors", Axis1Layout, 0, 2));
            Errors((GroupBox)Axis1Layout.Controls[2], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("Troubleshooting", Axis1Layout, 0, 3));
            TroubleShooting((GroupBox)Axis1Layout.Controls[3], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("Start-up Sequence", Axis1Layout, 0, 4));
            StartUpSequence((GroupBox)Axis1Layout.Controls[4], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("Additional Settings", Axis1Layout, 0, 5));
            AdditionalSettings((GroupBox)Axis1Layout.Controls[5], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("Application Setup", Axis1Layout, 0, 6));
            ApplicationSetup((GroupBox)Axis1Layout.Controls[6], "axis1");

            Axis1Layout.Controls.Add(CreateGroupBox("PID Tuning", Axis1Layout, 0, 7));
            PIDTuning((GroupBox)Axis1Layout.Controls[7], "axis1");
        }

        ////////////////////////////////////////////////////////////////////////////////// GUI sections //////////////////////////////////////////////////////////////////////////////////

        private GroupBox CreateGroupBox(string text, TableLayoutPanel parent, int column, int row)
        {
            GroupBox groupBox = new GroupBox();
            groupBox.Text = text;
            groupBox.AutoSize = true;
            groupBox.Dock = DockStyle.Fill;
            parent.Controls.Add(groupBox, column, row);
            parent.SetColumnSpan(groupBox, 5);

            TableLayoutPanel TableLayout = new TableLayoutPanel();
            TableLayout.ColumnCount = 4;
            TableLayout.RowCount = 1;
            TableLayout.Dock = DockStyle.Fill;
            TableLayout.AutoSize = true;
            TableLayout.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            TableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 32F));
            TableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            TableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 8F));
            TableLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 30F));
            groupBox.Controls.Add(TableLayout);

            return groupBox;
        }

        private void EssentialSettings(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            DropdownSetting(TableLayout, "Motor type", $"{axis}.motor.config.motor_type", "Select the type of motor you are using, brushless (BLDC), asynchronous (ACIM) or GIMBAL", MotorTypes);

            var settings1 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"label", "Pole pairs"},
                    {"path", $"{axis}.motor.config.pole_pairs"},
                    {"info", "Number of pole pairs of the motor. This is equal to the number of permanent magnets in the rotor divided by 2."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Current limit [A]"},
                    {"path", $"{axis}.motor.config.current_lim"},
                    {"info", "Maximum motor current, this should be less than the rated current of the motor. If you need more than 60A, you must change the current range."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Calibration current [A]"},
                    {"path", $"{axis}.motor.config.calibration_current"},
                    {"info", "Current during motor calibration."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity limit [rev/s]"},
                    {"path", $"{axis}.controller.config.vel_limit"},
                    {"info", "Maximum velocity of the motor in revolutions per second."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity limit tolerance [ratio]"},
                    {"path", $"{axis}.controller.config.vel_limit_tolerance"},
                    {"info", "Maximum velocity overshoot in comparison to the velocity limit."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Brake resistance [Ohm]"},
                    {"path", "config.brake_resistance"},
                    {"info", "Resistance of the brake resistor, this is used to dissipate the energy of the motor when decelerating. If you don't have a brake resistor, you can set this to 0 (FW0.5.1) or use the dropdown below (FW0.5.6)."}
                }
            };

            foreach (var setting in settings1)
            {
                TextBoxSetting(TableLayout, setting["label"], setting["path"], setting["info"]);
            }

            DropdownSetting(TableLayout, "Brake resistor", "config.enable_brake_resistor", "Enable/disable brake resistor for both axes, 'Invalid' means the firmware does not support this command.", Bools);

            var settings2 = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"label", "Encoder type"},
                    {"path", $"{axis}.encoder.config.mode"},
                    {"info", "Select the type of encoder you are using."},
                    {"options", EncoderModes}
                },
                new Dictionary<string, object>
                {
                    {"label", "Use index"},
                    {"path", $"{axis}.encoder.config.use_index"},
                    {"info", "Enable/disable the use of the index signal, this is only used for incremental encoders with index pulse."},
                    {"options", Bools}
                }
            };

            foreach (var setting in settings2)
            {
                DropdownSetting(TableLayout, (string)setting["label"], (string)setting["path"], (string)setting["info"], (Dictionary<string, long>)setting["options"]);
            }

            var settings3 = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"label", "Chip select GPIO pin"},
                    {"path", $"{axis}.encoder.config.abs_spi_cs_gpio_pin"},
                    {"info", "GPIO pin used for the SPI chip select signal. This is only used when using an SPI encoder. Power cycle may be required for changes to take effect."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Encoder CPR"},
                    {"path", $"{axis}.encoder.config.cpr"},
                    {"info", "Number of counts per revolution, this is the same as 4 times the pulses per revolution (PPR)."}
                }
            };

            foreach (var setting in settings3)
            {
                TextBoxSetting(TableLayout, setting["label"], setting["path"], setting["info"]);
            }

            SaveAndReboot(TableLayout, 1, 3);
        }

        private void TestSettings(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            ButtonCommand(TableLayout, "Motor calibration", $"w {axis}.requested_state {AxisStates["MOTOR_CALIBRATION"].ToString()}", 0, 5);
            ButtonCommand(TableLayout, "Encoder offset calibration", $"w {axis}.requested_state {AxisStates["ENCODER_OFFSET_CALIBRATION"].ToString()}", 0, 5);
            ButtonCommand(TableLayout, "Full calibration sequence", $"w {axis}.requested_state {AxisStates["FULL_CALIBRATION_SEQUENCE"].ToString()}", 0, 5);
            ButtonCommand(TableLayout, "Closed loop control", $"w {axis}.requested_state {AxisStates["CLOSED_LOOP_CONTROL"].ToString()}", 0, 5);
            ButtonCommand(TableLayout, "Idle", $"w {axis}.requested_state {AxisStates["IDLE"].ToString()}", 0, 5);
            TestPositionButton(TableLayout, 0, 5, axis);
            TestVelocityButton(TableLayout, 0, 5, axis);
        }

        private void Errors(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            UpdateErrors(TableLayout, 0, 5);
            ClearErrors(TableLayout,0, 5, axis);
            ErrorDisplay(TableLayout, "Board error", "error", BoardErrors);
            ErrorDisplay(TableLayout, "Axis error", $"{axis}.error", AxisErrors);
            ErrorDisplay(TableLayout, "Motor error", $"{axis}.motor.error", MotorErrors);
            ErrorDisplay(TableLayout, "Encoder error", $"{axis}.encoder.error", EncoderErrors);
            ErrorDisplay(TableLayout, "Controller error", $"{axis}.controller.error", ControllerErrors);
        }

        private void TroubleShooting(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            var settings = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"label", "Calibration voltage [V]"},
                    {"path", $"{axis}.motor.config.resistance_calib_max_voltage"},
                    {"info", "The default value is not enough for high resistance motors. In general, you need resistance_calib_max_voltage > calibration_current * phase_resistance resistance_calib_max_voltage < 0.5 * vbus_voltage"}
                },
                new Dictionary<string, string>
                {
                    {"label", "Scan distance [rad]"},
                    {"path", $"{axis}.encoder.config.calib_scan_distance"},
                    {"info", "The rotation amount during the calibration movement, for low CPR position sensors, this value should be increased. If you want a scan distance of n mechanical revolutions, you need to set this to n * 2 * pi * pole pairs."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Current control bandwidth [rad/s]"},
                    {"path", $"{axis}.motor.config.current_control_bandwidth"},
                    {"info", "Low KV motors may vibrate in position hold, reducing the current control bandwidth may solve this. This uses a biquad filter, so the resulting phase margin is actually half of what you set."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Encoder bandwidth [rad/s]"},
                    {"path", $"{axis}.encoder.config.bandwidth"},
                    {"info", "This is the cutoff frequency of the encoder velocity estimation. It should be set to a bit less than the electrical bandwidth of the motor."}
                }
            };

            foreach (var setting in settings)
            {
                TextBoxSetting(TableLayout, setting["label"], setting["path"], setting["info"]);
            }

            SaveAndReboot(TableLayout, 1, 3);

        }

        private void StartUpSequence(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            var settings = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"label", "Startup motor calibration"},
                    {"path", $"{axis}.config.startup_motor_calibration"},
                    {"info", "If true, the motor will be calibrated on startup."},
                    {"options", Bools}
                },
                new Dictionary<string, object>
                {
                    {"label", "Save motor calibration"},
                    {"path", $"{axis}.motor.config.pre_calibrated"},
                    {"info", "If true, the motor calibration will be saved to the board so you don't need motor startup calibration. Before setting this to true, you must perform motor calibration."},
                    {"options", Bools}
                }
            };

            foreach (var setting in settings)
            {
                DropdownSetting(TableLayout, (string)setting["label"], (string)setting["path"], (string)setting["info"], (Dictionary<string, long>)setting["options"]);
            }

            ButtonCommand(TableLayout, "Motor calibration", $"w {axis}.requested_state {AxisStates["MOTOR_CALIBRATION"].ToString()}", 1, 3);

            var settings2 = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"label", "Startup encoder offset calibration"},
                    {"path", $"{axis}.config.startup_encoder_offset_calibration"},
                    {"info", "If true, the encoder offset calibration will be performed on startup."},
                    {"options", Bools}
                },
                new Dictionary<string, object>
                {
                    {"label", "Save encoder offset calibration"},
                    {"path", $"{axis}.encoder.config.pre_calibrated"},
                    {"info", "If true, the encoder offset calibration will be saved to the board so you don't need encoder offset calibration. This can only be done when using an encoder with index, hall effect sensors or an absolute encoder, when using an encoder with index, you must also enable index search on startup.  Before setting this to true, you must perform encoder offset calibration."},
                    {"options", Bools}
                }
            };

            foreach (var setting in settings2)
            {
                DropdownSetting(TableLayout, (string)setting["label"], (string)setting["path"], (string)setting["info"], (Dictionary<string, long>)setting["options"]);
            }

            ButtonCommand(TableLayout, "Encoder offset calibration", $"w {axis}.requested_state {AxisStates["ENCODER_OFFSET_CALIBRATION"].ToString()}", 1, 3);

            var settings3 = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"label", "Startup encoder index search"},
                    {"path", $"{axis}.config.startup_encoder_index_search"},
                    {"info", "If true, the encoder index search will be performed on startup."},
                    {"options", Bools}
                },
                new Dictionary<string, object>
                {
                    {"label", "Startup closed loop control"},
                    {"path", $"{axis}.config.startup_closed_loop_control"},
                    {"info", "If true, the closed loop control will be started on startup"},
                    {"options", Bools}
                }
            };

            foreach (var setting in settings3)
            {
                DropdownSetting(TableLayout, (string)setting["label"], (string)setting["path"], (string)setting["info"], (Dictionary<string, long>)setting["options"]);
            }

            SaveAndReboot(TableLayout, 1, 3);
        }

        private void AdditionalSettings(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            var settings = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"label", "Calibration current [A]"},
                    {"path", $"{axis}.motor.config.calibration_current"},
                    {"info", "Current during motor calibration."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Encoder offset calib vel [rad/s]"},
                    {"path", $"{axis}.encoder.config.calib_scan_omega"},
                    {"info", "The velocity at which the motor turns during encoder offset calibration. This is in electrical so 2*2*pi*#polepairs" }
                },
                new Dictionary<string, string>
                {
                    {"label", "Torque constant [Nm/A]"},
                    {"path", $"{axis}.motor.config.torque_constant"},
                    {"info", "Torque constant KT of the motor, this is equal to 8.27/KV where KV is the motor velocity constant in rpm/V. If you want to input current instead of torque in torque control mode, you can set this to 1 Nm/A."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity limit [rev/s]"},
                    {"path", $"{axis}.controller.config.vel_limit"},
                    {"info", "Maximum velocity of the motor in rotations per second."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity limit tolerance [ratio]"},
                    {"path", $"{axis}.controller.config.vel_limit_tolerance"},
                    {"info", "Maximum velocity tolerance in rotations per second. This is used to determine when the axis is stationary."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Current range [A]"},
                    {"path", $"{axis}.motor.config.requested_current_range"},
                    {"info", "This changes the amplifier gain to get a more accurate current measurement. The default is 60 A and the maximum is 120 A."}
                }

            };

            foreach (var setting in settings)
            {
                TextBoxSetting(TableLayout, setting["label"], setting["path"], setting["info"]);
            }

            SaveAndReboot(TableLayout, 1, 3);
        }

        private void ApplicationSetup(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            var settings = new List<Dictionary<string, object>>
            {
                new Dictionary<string, object>
                {
                    {"label", "Control mode"},
                    {"path", $"{axis}.controller.config.control_mode"},
                    {"info", "Select the control mode you want to use."},
                    {"options", ControlModes}
                },
                new Dictionary<string, object>
                {
                    {"label", "Input mode"},
                    {"path", $"{axis}.controller.config.input_mode"},
                    {"info", "Select the input mode you want to use."},
                    {"options", InputModes}
                },
            };

            foreach (var setting in settings)
            {
                DropdownSetting(TableLayout, (string)setting["label"], (string)setting["path"], (string)setting["info"], (Dictionary<string, long>)setting["options"]);
            }

            var settings2 = new List<Dictionary<String, String>>
            {
                new Dictionary<string, string>
                {
                    {"label", "Input filter bandwidth [Hz]"},
                    {"path", $"{axis}.controller.config.input_filter_bandwidth"},
                    {"info", "The bandwidth of the input filter, this is used to filter the input signal."},
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity limit [rev/s]"},
                    {"path", $"{axis}.controller.config.vel_limit"},
                    {"info", "The maximum velocity of the motor in rotations per second."},
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity limit tolerance [ratio]"},
                    {"path", $"{axis}.controller.config.vel_limit_tolerance"},
                    {"info", "The maximum velocity tolerance in rotations per second. This is used to determine when the axis} is stationary."},
                }
            };

            foreach (var setting in settings2)
            {
                TextBoxSetting(TableLayout, setting["label"], setting["path"], setting["info"]);
            }

            SaveAndReboot(TableLayout, 1, 3);
        }

        private void PIDTuning(GroupBox groupBox, string axis)
        {
            TableLayoutPanel TableLayout = (TableLayoutPanel)groupBox.Controls[0];

            var settings = new List<Dictionary<string, string>>
            {
                new Dictionary<string, string>
                {
                    {"label", "Position gain"},
                    {"path", $"{axis}.controller.config.pos_gain"},
                    {"info", "The proportional gain of the position controller."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity gain"},
                    {"path", $"{axis}.controller.config.vel_gain"},
                    {"info", "The proportional gain of the velocity controller."}
                },
                new Dictionary<string, string>
                {
                    {"label", "Velocity integrator gain"},
                    {"path", $"{axis}.controller.config.vel_integrator_gain"},
                    {"info", "The integral gain of the velocity controller."}
                }
            };

            foreach (var setting in settings)
            {
                TextBoxSetting(TableLayout, setting["label"], setting["path"], setting["info"]);
            }

            SaveAndReboot(TableLayout, 1, 3);
        }

        ////////////////////////////////////////////////////////////////////////////////// GUI rows //////////////////////////////////////////////////////////////////////////////////

        private void TextBoxSetting(TableLayoutPanel TableLayout, string label, string path, string tooltip)
        {
            LabelObject labelObject = new LabelObject(TableLayout, label, tooltip, CurrentRow, 0);
            TextBoxObject TextBoxObject = new TextBoxObject(TableLayout, CurrentRow, 1, 1);
            DisplayObject displayObject = new DisplayObject(TableLayout, CurrentRow, 3, 1);
            var temp = new Dictionary<string, object>
            {
                {"displayobject", displayObject},
                {"path", path},
                {"options", null}
            };
            DisplayObjects.Add(temp);
            SendButtonObject sendButtonObject = new SendButtonObject(TableLayout, CurrentRow, 2, (s, e) => SubmitTextBox(TextBoxObject.TextBox, displayObject.TextBox, path));
            TextBoxObject.TextBox.KeyDown += (s, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    SubmitTextBox(TextBoxObject.TextBox, displayObject.TextBox, path);
                    e.Handled = true;
                    e.SuppressKeyPress = true;
                }
            };
            CurrentRow++;
        }

        private void DropdownSetting(TableLayoutPanel TableLayout, string label, string path, string tooltip, Dictionary<string, long> options)
        {
            LabelObject labelObject = new LabelObject(TableLayout, label, tooltip, CurrentRow, 0);
            DropdownObject dropdownObject = new DropdownObject(TableLayout, CurrentRow, 1, 1, options);
            DisplayObject displayObject = new DisplayObject(TableLayout, CurrentRow, 3, 1);
            var temp = new Dictionary<string, object>
            {
                {"displayobject", displayObject},
                {"path", path},
                {"options", options}
            };
            DisplayObjects.Add(temp);
            SendButtonObject sendButtonObject = new SendButtonObject(TableLayout, CurrentRow, 2, (s, e) => SubmitComboBox(dropdownObject.ComboBox, displayObject.TextBox, path, options));
            CurrentRow++;
        }

        private void ErrorDisplay(TableLayoutPanel TableLayout, string label, string path, Dictionary<string, long> errors)
        {
            LabelObject LabelObject = new LabelObject(TableLayout, label, "", CurrentRow, 0);
            DisplayObject DisplayObject = new DisplayObject(TableLayout, CurrentRow, 1, 3);
            var temp = new Dictionary<string, object>
            {
                {"displayobject", DisplayObject},
                {"path", path},
                {"options", errors}
            };
            DisplayObjects.Add(temp);
            ErrorDisplayObjects.Add(temp);
            CurrentRow++;
        }

        private void ButtonCommand(TableLayoutPanel TableLayout, string text, string command, int column, int columnspan)
        {
            ButtonObject buttonObject = new ButtonObject(TableLayout, CurrentRow, column, columnspan, SerialPort1, command, text);
            CurrentRow++;
        }

        private void UpdateErrors(TableLayoutPanel TableLayout, int column, int columnspan)
        {
            ButtonObject ButtonObject = new ButtonObject(TableLayout, CurrentRow, column, columnspan, SerialPort1, "", "Update errors");
            ButtonObject.Button.Click += (s, e) =>
            {
                foreach (var item in ErrorDisplayObjects)
                {
                    var displayObject = (DisplayObject)item["displayobject"];
                    var options = (Dictionary<string, long>)item["options"];
                    var path = item["path"].ToString();

                    string response = SendCommandAndReadResponse($"r {path}");

                    if (FirmwareVersion == 51 && path == "error")
                    {
                        displayObject.TextBox.Text = "Unknown";
                    }
                    else
                    {
                        displayObject.TextBox.Text = GetKeyByValue(options, int.Parse(response));
                    }
                }
            };
            CurrentRow++;
        }

        private void ClearErrors(TableLayoutPanel TableLayout, int column, int columnspan, string axis)
        {
            ButtonObject ButtonObject = new ButtonObject(TableLayout, CurrentRow, column, columnspan, SerialPort1, "", "Clear errors");
            ButtonObject.Button.Click += (s, e) =>
            {
                try
                {
                    if (SerialPort1.IsOpen)
                    {
                        if (FirmwareVersion == 56)
                        {
                            SerialPort1.WriteLine($"w error 0" + Environment.NewLine);
                        }
                        SerialPort1.WriteLine($"w {axis}.error 0" + Environment.NewLine);
                        SerialPort1.WriteLine($"w {axis}.motor.error 0" + Environment.NewLine);
                        SerialPort1.WriteLine($"w {axis}.encoder.error 0" + Environment.NewLine);
                        SerialPort1.WriteLine($"w {axis}.controller.error 0" + Environment.NewLine);
                        System.Threading.Thread.Sleep(100);

                        foreach (var item in ErrorDisplayObjects)
                        {
                            var displayObject = (DisplayObject)item["displayobject"];
                            var options = (Dictionary<string, long>)item["options"];
                            var path = item["path"].ToString();

                            string response = SendCommandAndReadResponse($"r {path}");

                            if (FirmwareVersion == 51 && path == "error")
                            {
                                displayObject.TextBox.Text = "Unknown";
                            }
                            else
                            {
                                displayObject.TextBox.Text = GetKeyByValue(options, int.Parse(response));
                            }
                        }
                    }
                    else
                    {
                        HandleReconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            };
            CurrentRow++;
        }

        private void SaveAndReboot(TableLayoutPanel TableLayout, int column, int columnspan)
        {
            ButtonObject ButtonObject = new ButtonObject(TableLayout, CurrentRow, column, columnspan, SerialPort1, "", "Save and reboot");
            ButtonObject.Button.Click += (s, e) =>
            {
                try
                {
                    if (SerialPort1.IsOpen)
                    {
                        SerialPort1.WriteLine("ss" + Environment.NewLine);
                        System.Threading.Thread.Sleep(1000);
                        HandleReconnectAsync();
                    }
                    else
                    {
                        HandleReconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            };
        }

        private void TestPositionButton(TableLayoutPanel TableLayout, int column, int columnspan, string axis)
        {
            ButtonObject ButtonObject = new ButtonObject(TableLayout, CurrentRow, column, columnspan, SerialPort1, "", "Test Position");
            ButtonObject.Button.Click += (s, e) =>
            {
                try
                {
                    if (SerialPort1.IsOpen)
                    {
                        // set closed loop control
                        SerialPort1.WriteLine($"w {axis}.requested_state {AxisStates["CLOSED_LOOP_CONTROL"].ToString()}");
                        SerialPort1.WriteLine($"w {axis}.controller.config.control_mode {ControlModes["CTRL_MODE_POSITION_CONTROL"]}" + Environment.NewLine);
                        System.Threading.Thread.Sleep(10);
                        SerialPort1.WriteLine($"w {axis}.controller.input_pos 1" + Environment.NewLine);
                        System.Threading.Thread.Sleep(500);
                        SerialPort1.WriteLine($"w {axis}.controller.input_pos 0" + Environment.NewLine);
                        System.Threading.Thread.Sleep(500);
                        SerialPort1.WriteLine($"w {axis}.requested_state {AxisStates["IDLE"].ToString()}" + Environment.NewLine);
                    }
                    else
                    {
                        HandleReconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            };
            CurrentRow++;
        }

        private void TestVelocityButton(TableLayoutPanel TableLayout, int column, int columnspan, string axis)
        {
            ButtonObject ButtonObject = new ButtonObject(TableLayout, CurrentRow, column, columnspan, SerialPort1, "", "Test Velocity");
            ButtonObject.Button.Click += (s, e) =>
            {
                try
                {
                    if (SerialPort1.IsOpen)
                    {
                        SerialPort1.WriteLine($"w {axis}.requested_state {AxisStates["CLOSED_LOOP_CONTROL"].ToString()}" + Environment.NewLine);
                        SerialPort1.WriteLine($"w {axis}.controller.config.control_mode {ControlModes["CTRL_MODE_VELOCITY_CONTROL"]}" + Environment.NewLine);
                        System.Threading.Thread.Sleep(10);
                        SerialPort1.WriteLine($"w {axis}.controller.input_vel 1" + Environment.NewLine);
                        System.Threading.Thread.Sleep(1000);
                        SerialPort1.WriteLine($"w {axis}.controller.input_vel -1" + Environment.NewLine);
                        System.Threading.Thread.Sleep(1000);
                        SerialPort1.WriteLine($"w {axis}.controller.input_vel 0" + Environment.NewLine);
                        SerialPort1.WriteLine($"w {axis}.requested_state {AxisStates["IDLE"].ToString()}" + Environment.NewLine);
                    }
                    else
                    {
                        HandleReconnectAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Error");
                }
            };
            CurrentRow++;
        }

        ////////////////////////////////////////////////////////////////////////////////// functions //////////////////////////////////////////////////////////////////////////////////

        private string SendCommandAndReadResponse(string command)
        {
            try
            {
                if (SerialPort1.IsOpen)
                {
                    SerialPort1.WriteLine(command + Environment.NewLine);
                    System.Threading.Thread.Sleep(10);

                    string rawResponse = SerialPort1.ReadExisting().Trim();

                    if (rawResponse.Contains("invalid", StringComparison.OrdinalIgnoreCase))
                        return "2";

                    return Regex.Replace(rawResponse, "[^0-9.\\-]", "");
                }
                else
                {
                    HandleReconnectAsync();
                    return string.Empty;
                }
            }
            catch
            {
                return string.Empty;
            }
        }


        private string GetKeyByValue(Dictionary<string, long> dict, int value)
        {
            if (dict == Bools)
            {
                dict = new Dictionary<string, long>
                {
                    {"False", 0},
                    {"True", 1},
                    {"Invalid", 2}
                };
            }
            foreach (var kvp in dict)
            {
                if (kvp.Value == value)
                {
                    return kvp.Key;
                }
            }
            return null;
        }

        ////////////////////////////////////////////////////////////////////////////////// event handlers //////////////////////////////////////////////////////////////////////////////////

        private void SubmitTextBox(TextBox entry, TextBox display, string path)
        {
            try
            {
                if (SerialPort1.IsOpen)
                {
                    string command = $"w {path} {entry.Text}";
                    SerialPort1.WriteLine(command + Environment.NewLine);
                    string response = SendCommandAndReadResponse($"r {path}");
                    display.Text = response.Trim();
                    entry.Clear();
                }
                else
                {
                    HandleReconnectAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void SubmitComboBox(ComboBox entry, TextBox display, string path, Dictionary<string, long> options)
        {
            try
            {
                if (SerialPort1.IsOpen)
                {
                    string command = $"w {path} {options[entry.SelectedItem.ToString()]}";
                    SerialPort1.WriteLine(command + Environment.NewLine);
                    string response = SendCommandAndReadResponse($"r {path}");
                    display.Text = GetKeyByValue(options, int.Parse(response));
                    entry.SelectedIndex = -1;
                }
                else
                {
                    HandleReconnectAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void Refresh_Click(object sender, EventArgs e)
        {
            PortSelection.Items.Clear();
            var portNames = SerialPort.GetPortNames();

            foreach (var portName in portNames)
            {
                string deviceName = GetDeviceName(portName);
                PortSelection.Items.Add($"{portName} - {deviceName}");
            }

            if (PortSelection.Items.Count > 0)
            {
                PortSelection.SelectedIndex = 0;
            }
        }

        private void Connect_Click(object sender, EventArgs e)
        {
            try
            {
                SerialPort1.PortName = PortSelection.SelectedItem.ToString().Split(' ')[0];
                SerialPort1.BaudRate = 115200;
                SerialPort1.Open();

                ConnectButton.Enabled = false;
                DisconnectButton.Enabled = true;

                // Query firmware revision
                string fwRevStr = SendCommandAndReadResponse("r fw_version_revision");
                if (fwRevStr == "6")
                    FirmwareVersion = 56;
                else if (fwRevStr == "0")
                    FirmwareVersion = 51;

                foreach (var item in DisplayObjects)
                {
                    if (item["options"] == null)
                    {
                        var displayObject = (DisplayObject)item["displayobject"];
                        string response = SendCommandAndReadResponse($"r {item["path"]}");
                        displayObject.TextBox.Text = response;
                    }
                    else
                    {
                        var displayObject = (DisplayObject)item["displayobject"];
                        string response = SendCommandAndReadResponse($"r {item["path"]}");

                        if (FirmwareVersion == 51 && item["path"] == "error")
                        {
                            displayObject.TextBox.Text = "Unknown";
                        }
                        else
                        {
                            displayObject.TextBox.Text = GetKeyByValue((Dictionary<string, long>)item["options"], int.Parse(response));
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
                Refresh_Click(null, null);
            }
        }

        private void Disconnect_Click(object sender, EventArgs e)
        {
            try
            {
                SerialPort1.Close();
                ConnectButton.Enabled = true;
                DisconnectButton.Enabled = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private void EraseConfiguration_Click(object sender, EventArgs e)
        {
            try
            {
                if (SerialPort1.IsOpen)
                {
                    string command = "se";
                    SerialPort1.WriteLine(command + Environment.NewLine);
                    SerialPort1.Close();
                    System.Threading.Thread.Sleep(1000);
                    Connect_Click(null, null);
                }
                else
                {
                    HandleReconnectAsync();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, "Error");
            }
        }

        private string GetDeviceName(string portName)
        {
            try
            {
                using (var searcher = new ManagementObjectSearcher($"SELECT * FROM Win32_PnPEntity WHERE Caption LIKE '%({portName})%'"))
                {
                    foreach (var obj in searcher.Get())
                    {
                        return obj["Caption"].ToString();
                    }
                }
            }
            catch (Exception)
            {
                return "Unknown Device";
            }

            return "Unknown Device";
        }

        private async Task<bool> TryReconnectSerialPortAsync(int maxRetries, int delayMs)
        {
            for (int i = 0; i < maxRetries; i++)
            {
                try
                {
                    if (SerialPort1.IsOpen) SerialPort1.Close();

                    SerialPort1.PortName = PortSelection.SelectedItem.ToString().Split(' ')[0];
                    SerialPort1.BaudRate = 115200;

                    SerialPort1.Open();

                    foreach (var item in DisplayObjects)
                    {
                        if (item["options"] == null)
                        {
                            var displayObject = (DisplayObject)item["displayobject"];
                            string response = SendCommandAndReadResponse($"r {item["path"]}");
                            displayObject.TextBox.Text = response;
                        }
                        else
                        {
                            var displayObject = (DisplayObject)item["displayobject"];
                            string response = SendCommandAndReadResponse($"r {item["path"]}");

                            if (FirmwareVersion == 51 && item["path"] == "error")
                            {
                                displayObject.TextBox.Text = "Unknown";
                            }
                            else
                            {
                                displayObject.TextBox.Text = GetKeyByValue((Dictionary<string, long>)item["options"], int.Parse(response));
                            }
                        }
                    }

                    return true;
                }
                catch (System.IO.FileNotFoundException) { await Task.Delay(delayMs); }
                catch (UnauthorizedAccessException) { await Task.Delay(delayMs); }
                catch (System.IO.IOException) { await Task.Delay(delayMs); }
                catch (Exception) { await Task.Delay(delayMs); }
            }
            return false;
        }

        private async Task HandleReconnectAsync()
        {
            DisconnectButton.Enabled = false;
            ConnectButton.Enabled = false;

            if (SerialPort1.IsOpen)
                SerialPort1.Close();

            bool reconnected = await TryReconnectSerialPortAsync(5, 1000);

            if (reconnected)
            {
                ConnectButton.Enabled = false;
                DisconnectButton.Enabled = true;
            }
            else
            {
                ConnectButton.Enabled = true;
                DisconnectButton.Enabled = false;
                MessageBox.Show("Failed to reconnect after device reboot/disconnect.", "Error");
            }
        }

        ////////////////////////////////////////////////////////////////////////////////// GUI classes //////////////////////////////////////////////////////////////////////////////////

        public class LabelObject
        {
            public Label Label;

            public LabelObject(TableLayoutPanel TableLayout, string text, string tooltip, int row, int column)
            {
                Label = new Label {Text = text, Dock = DockStyle.Fill};
                Label.TextAlign = ContentAlignment.MiddleLeft;
                TableLayout.Controls.Add(Label, column, row);
                if (!string.IsNullOrEmpty(tooltip))
                {
                    ToolTip toolTip = new ToolTip();
                    toolTip.SetToolTip(Label, tooltip);
                }
            }
        }

        public class TextBoxObject
        {
            public TextBox TextBox;

            public TextBoxObject(TableLayoutPanel TableLayout, int row, int column, int columnspan)
            {
                TextBox = new TextBox {Dock = DockStyle.Fill};
                TableLayout.Controls.Add(TextBox, column, row);
                TableLayout.SetColumnSpan(TextBox, columnspan);

            }
        }

        public class DisplayObject
        {
            public TextBox TextBox;

            public DisplayObject(TableLayoutPanel TableLayout, int row, int column, int columnSpan)
            {
                TextBox = new TextBox {Dock = DockStyle.Fill, ReadOnly = true};
                TableLayout.Controls.Add(TextBox, column, row);
                TableLayout.SetColumnSpan(TextBox, columnSpan);
            }
        }

        public class DropdownObject
        {
            public ComboBox ComboBox;

            public DropdownObject(TableLayoutPanel TableLayout, int row, int column, int columnspan, Dictionary<string, long> options)
            {
                ComboBox = new ComboBox {Dock = DockStyle.Fill, DropDownStyle = ComboBoxStyle.DropDownList};
                foreach (var option in options)
                {
                    ComboBox.Items.Add(option.Key);
                }
                TableLayout.Controls.Add(ComboBox, column, row);
                TableLayout.SetColumnSpan(ComboBox, columnspan);
            }
        }

        public class SendButtonObject
        {
            public Button Button;

            public SendButtonObject(TableLayoutPanel TableLayout, int row, int column, EventHandler eventHandler)
            {
                Button = new Button {Text = "Send", Dock = DockStyle.Fill};
                Button.Click += eventHandler;
                TableLayout.Controls.Add(Button, column, row);
            }
        }

        public class ButtonObject
        {
            public Button Button;

            public ButtonObject(TableLayoutPanel TableLayout, int row, int column, int columnspan, SerialPort SerialPort, string command, string text)
            {
                Button = new Button {Text = text, Dock = DockStyle.Fill};
                Button.Click += (s, e) =>
                {
                    try
                    {
                        if (SerialPort.IsOpen)
                        {
                            SerialPort.WriteLine(command + Environment.NewLine);
                        }
                        else
                        {
                            MessageBox.Show("Serial port is not open", "Error");
                        }
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, "Error");
                    };
                };
                TableLayout.Controls.Add(Button, column, row);
                TableLayout.SetColumnSpan(Button, columnspan);
            }
        }

        public class HeaderObject
        {
            public FlowLayoutPanel Panel;
            public LinkLabel Link;

            public HeaderObject(TableLayoutPanel TableLayout, int row, int column, int columnspan)
            {
                Label titlePrefix = new Label
                {
                    Text = "ODrive V3.X GUI by",
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleLeft
                };

                Link = new LinkLabel
                {
                    Text = "Stijns Projects",
                    AutoSize = true,
                    TextAlign = ContentAlignment.MiddleLeft,
                    LinkColor = Color.Black,
                    ActiveLinkColor = Color.Gray,
                    VisitedLinkColor = Color.Black
                };
                Link.LinkClicked += (s, e) =>
                {
                    var psi = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = "https://www.youtube.com/@StijnsProjects",
                        UseShellExecute = true
                    };
                    System.Diagnostics.Process.Start(psi);
                };

                Panel = new FlowLayoutPanel
                {
                    Dock = DockStyle.Fill,
                    AutoSize = true,
                    Anchor = AnchorStyles.Left,
                    FlowDirection = FlowDirection.LeftToRight,
                    WrapContents = false
                };

                Panel.Controls.Add(titlePrefix);
                Panel.Controls.Add(Link);

                TableLayout.Controls.Add(Panel, column, row);
                TableLayout.SetColumnSpan(Panel, columnspan);
            }
        }
    }
}