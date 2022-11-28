/////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
// ProjectName: DrawingWorld.csproj
// FileName: Form1.cs
// FileType: Visual C# source file
// Professor: Daniel Kopta
// Author: Kevin Matsui, Ee Jae Ahn
// Date: 11/29/2021
//////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////////
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using GameController;
using Newtonsoft.Json;
using TankWars;


namespace DrawingWorld
{
    /// <summary>
    /// Draws the view of the game, by adding a drawing panel object to the form
    /// Contains the event listeners necessary to fire controllers event handlers.
    /// </summary>
    public partial class Form1 : Form
    {
        // controller and panel for the form to use
        private Controller controller;
        private DrawingPanel panel;

        public Form1()
        {
            InitializeComponent();

            // sets the form to open with given size
            this.Width = 900;
            this.Height = 950;

            // set textBox text
            serverTextBox.Text = "localhost";
            nameTextBox.Text = "player1";

            // initialize the controller and its event handlers
            controller = new Controller();
            controller.updateArrived += OnFrame;
            controller.Error += OnError;

            // create the drawing panel and place it on the form with the view size 900x900
            panel = new DrawingPanel(controller.TheWorld, controller);
            panel.Location = new Point(0, 50);
            panel.Size = new Size(900, 900);
            this.Controls.Add(panel);
            panel.BackColor = Color.Black;

            // sets the event handlers to the corresponding event
            this.KeyDown += Form1_KeyDown;
            this.KeyUp += Form1_KeyUp;
            panel.MouseDown += Form1_MouseDown;
            panel.MouseUp += Form1_MouseUp;
            panel.MouseMove += Form1_MouseMove;
        }

        /// <summary>
        /// Redraws the view each time new data is received
        /// </summary>
        private void OnFrame()
        {
            // redraws the view if the world and worldsize is initialized
            if (controller.TheWorld != null && controller.WorldSize != 0)
            {
                // invokes the onpaint method to redraw the world on each receive
                try
                {
                    this.Invoke(new MethodInvoker(() => Invalidate(true)));
                }
                catch
                {
                }
            }
        }

        /// <summary>
        /// Displays error messages that occur during the connection process
        /// or while connected. 
        /// </summary>
        /// <param name="err"></param>
        private void OnError(string err)
        {
            // call the helper method to handle the error
            this.Invoke(new MethodInvoker(() => OnErrorHelper(err)));
        }
        /// <summary>
        /// Helper method for on error, shows the message and sets the connect button and
        /// serverTextBox back to enabled to allow users to attempt to reconnect
        /// </summary>
        /// <param name="err"></param>
        private void OnErrorHelper(string err)
        {
            // show message
            MessageBox.Show(err);
            // re-enable the connect button and server text box
            connectButton.Enabled = true;
            serverTextBox.Enabled = true;
        }


        /// <summary>
        /// What to be done when the user clicks the connect button
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void ConnectButton_Click(object sender, EventArgs e)
        {
            // checks that the textbox isn't empty
            if (serverTextBox.Text == "")
            {
                OnError("Please enter a server address");
                return;
            }
            // checks that the name entered is between 1 and 16 characters.
            else if (nameTextBox.Text.Length > 16 || nameTextBox.Text.Length == 0)
            {
                OnError("Player name must be between 1 and 16 characters! ");
                return;
            }

            // Disable the controls and try to connect
            connectButton.Enabled = false;
            serverTextBox.Enabled = false;
            nameTextBox.Enabled = false;

            // passing server address to the controller's connecting client method
            controller.Connect(nameTextBox.Text, serverTextBox.Text);

        }

        /// <summary>
        /// What is done when a user presses a keydown on the form.
        /// Adds the movement command string to the move commands stack.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyDown(object sender, KeyEventArgs e)
        {
            // push the corresponding movement command string to the movement commands stack in the controller
            if (e.KeyCode == Keys.W)
            {
                controller.moveCommands.Push("up");
            }
            else if (e.KeyCode == Keys.A)
            {
                controller.moveCommands.Push("left");

            }
            else if (e.KeyCode == Keys.S)
            {
                controller.moveCommands.Push("down");

            }
            else if (e.KeyCode == Keys.D)
            {
                controller.moveCommands.Push("right");

            }

            controller.HandleMoveRequest();  // set the moving field of the controller commands object
            // Prevent other key handlers from running
            e.SuppressKeyPress = true;
            e.Handled = true;
        }

        /// <summary>
        /// This method handles what happens when the user moves the mouse on the form, 
        /// fires event in controller to change control command of the tdir to where
        /// the mouse moved
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseMove(object sender, MouseEventArgs e)
        {
            // if the world is not empty 
            if (controller.TheWorld.tanks.Count > 0)
            {
                // sets the vector to the location of the mouse in the view space coordinates
                // by subtracting the x and y coordinates by half of the view size
                Vector2D vector = new Vector2D(e.X - 900/2, e.Y - 900/2);

                // normalize the vector
                vector.Normalize();

                // fire the controller event to change the control command
                controller.HandleMouseMoveRequest(vector);
            }
        }

        /// <summary>
        /// This method removes the corresponding movement command from the
        /// movement commands stack in the controller.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_KeyUp(object sender, KeyEventArgs e)
        {
            // Calls the controller Key up handler with given direction string
            if(e.KeyCode == Keys.W)
            {
                controller.HandleKeyUpRequest("up");
            }
            else if (e.KeyCode == Keys.A)
            {
                controller.HandleKeyUpRequest("left");

            }
            else if(e.KeyCode == Keys.D)
            {
                controller.HandleKeyUpRequest("right");

            }
            else if(e.KeyCode == Keys.S)
            {
                controller.HandleKeyUpRequest("down");

            }
            controller.HandleMoveRequest();  // handles the movement request when key is raised

        }

        /// <summary>
        /// This method sets the fire control command back to none when a mouse button
        /// is lifted
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseUp(object sender, MouseEventArgs e)
        {
            controller.HandleFireRequest("none");
        }

        /// <summary>
        /// This method sets the fire control command to main or alt depending on
        /// if a left click or right click is detected on the panel.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Form1_MouseDown(object sender, MouseEventArgs e)
        {
            // left click is a normal projectile
            if (e.Button == MouseButtons.Left)
            {
                controller.HandleFireRequest("main");
                
            } 
            // right click is a beam
            else if (e.Button == MouseButtons.Right)
            {
                controller.HandleFireRequest("alt");
            }
        }

    }
}
