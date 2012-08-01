/*
 *  Copyright 2012 Tamme Schichler <tammeschichler@googlemail.com>
 * 
 *  This file is part of TQ Animation Cropper.
 *
 *  TQ.Animation is free software: you can redistribute it and/or modify
 *  it under the terms of the GNU General Public License as published by
 *  the Free Software Foundation, either version 3 of the License, or
 *  (at your option) any later version.
 *
 *  TQ Animation Cropper is distributed in the hope that it will be useful,
 *  but WITHOUT ANY WARRANTY; without even the implied warranty of
 *  MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 *  GNU General Public License for more details.
 *
 *  You should have received a copy of the GNU General Public License
 *  along with TQ Animation Cropper.  If not, see <http://www.gnu.org/licenses/>.
 */

using System;
using System.IO;
using System.Linq;
using System.Windows;
using Microsoft.Win32;
using TQ.Animation;

namespace TQ_Animation_Cropper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        private Animation _animation;

        public MainWindow()
        {
            InitializeComponent();
        }

        private void LoadButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
                          {
                              CheckFileExists = true,
                              FileName = "Animation",
                              DefaultExt = ".anm",
                              Filter = "TQ animations (.anm)|*.anm"
                          };

            if (dlg.ShowDialog() == true)
            {
                Load(dlg.FileName);
            }
        }

        private void Load(string path)
        {
            try
            {
                byte[] buffer = File.ReadAllBytes(path);
                _animation = Animation.Parse(buffer);

                UpdateLabels(path);

                saveButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading animation:\n" + ex.Message);
            }
        }

        private void UpdateLabels(string path)
        {
            animLabel.Content = Path.GetFileNameWithoutExtension(path) + ": " +
                                _animation.BoneAnimations.Length +
                                ((_animation.BoneAnimations.Length == 1) ? " Bone" : " Bones");
            if (_animation.BoneAnimations.Length > 0)
            {
                animLabel.Content = ((string) animLabel.Content) + ", " +
                                    _animation.BoneAnimations[0].BoneAnimationFrames.Length +
                                    ((_animation.BoneAnimations[0].BoneAnimationFrames.Length == 1)
                                         ? " Frame"
                                         : " Frames");
            }
        }

        private static void Save(Animation animation, string path)
        {
            try
            {
                File.WriteAllBytes(path, animation.GetBytes());
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error saving animation:\n" + ex.Message);
            }

            MessageBox.Show("Animation saved.");
        }

        private void SaveButtonClick(object sender, RoutedEventArgs e)
        {
            var dlg = new SaveFileDialog
                          {
                              AddExtension = true,
                              FileName = "New Animation",
                              DefaultExt = ".anm",
                              ValidateNames = true,
                              Filter = "TQ animations (.anm)|*.anm"
                          };

            if (dlg.ShowDialog() == true)
            {
                try
                {
                    int start = int.Parse(firstFrameTextBox.Text);
                    int end = start + int.Parse(frameCountTextBox.Text);

                    if (end == start - 1)
                    {
                        end = _animation.BoneAnimations[0].BoneAnimationFrames.Length;
                    }

                    // Crop animation
                    var animation =
                        new Animation(
                            _animation.BoneAnimations.Select(
                                b =>
                                new BoneAnimation(b.BoneName,
                                                  b.BoneAnimationFrames.Where((x, i) => i >= start && i < end).ToArray()))
                                .
                                ToArray(),
                            _animation.CallbackPoints.Where(c => c.Frame >= start && c.Frame < end).ToArray(),
                            _animation.Fps);

                    Save(animation, dlg.FileName);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error cropping animation:\n" + ex.Message);
                }
            }
        }
    }
}