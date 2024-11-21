using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace racingGame
{
    public partial class RacingView : Form
    {
        private Timer timer;
        private bool isEnd = false;
        private int playerCarPosition = 1; // Vị trí ban đầu của xe người chơi
        private readonly string playerCarImage = "playerCar.png";
        private readonly List<PictureBox> thirdRowBoxes;
        private readonly List<PictureBox> secondRowBoxes;
        private readonly List<PictureBox> firstRowBoxes;
        private int countScore = 0;
        private int highestScoreInt = 0;

        public RacingView()
        {
            InitializeComponent();

            // Cấu hình các hàng xe
            thirdRowBoxes = new List<PictureBox> { pictureBoxA, pictureBoxB, pictureBoxC };
            secondRowBoxes = new List<PictureBox> { pictureBox6, pictureBox7, pictureBox8 };
            firstRowBoxes = new List<PictureBox> { pictureBox3, pictureBox4, pictureBox5 };

            InitializePlayerCar();
        }

        private void InitializePlayerCar()
        {
            string carPath = GetCarImagePath(playerCarImage); // Đường dẫn ảnh xe người chơi
            if (File.Exists(carPath))
            {
                var playerBox = thirdRowBoxes[playerCarPosition];
                playerBox.BackgroundImage = Image.FromFile(carPath);
                playerBox.BackgroundImageLayout = ImageLayout.Stretch;
                playerBox.Tag = playerCarImage;
            }
        }


        private void MoveCarLeft()
        {
            if (playerCarPosition > 0) // Kiểm tra để không vượt ngoài biên
            {
                UpdatePlayerCarPosition(playerCarPosition - 1);
            }
        }


        private void MoveCarRight()
        {
            if (playerCarPosition < thirdRowBoxes.Count - 1) // Kiểm tra để không vượt ngoài biên
            {
                UpdatePlayerCarPosition(playerCarPosition + 1);
            }
        }

        private void UpdatePlayerCarPosition(int newPosition)
        {
            // Xóa xe ở vị trí cũ
            var currentBox = thirdRowBoxes[playerCarPosition];
            currentBox.BackgroundImage = null;
            currentBox.Tag = null;

            // Cập nhật vị trí mới
            playerCarPosition = newPosition;

            // Hiển thị xe ở vị trí mới
            InitializePlayerCar();
        }


        private async void ChangeRandomPictureBoxBackground()
        {
            int timeDelay = 1000;

            // Điều chỉnh timeDelay tùy theo điểm số
            if (countScore >= 4) timeDelay -= 300;
            if (countScore >= 8) timeDelay -= 300;
            if (countScore >= 12) timeDelay -= 300;

            // Clear các hàng trước khi cập nhật
            ClearRow(thirdRowBoxes);
            ClearRow(secondRowBoxes);
            ClearRow(firstRowBoxes);

            var carImages = new List<string> { "car2.png", "car3.png", "car4.png", "car5.png" };
            var random = new Random();

            // Chọn ngẫu nhiên 2 xe cho hàng 1
            var selectedCars = carImages.OrderBy(x => random.Next()).Take(2).ToList();
            var selectedBoxes = firstRowBoxes.OrderBy(x => random.Next()).Take(2).ToList();

            // Cập nhật hình ảnh cho các xe đã chọn trong hàng 1
            for (int i = 0; i < selectedBoxes.Count; i++)
            {
                string carPath = GetCarImagePath(selectedCars[i]);
                selectedBoxes[i].BackgroundImage = Image.FromFile(carPath);
                selectedBoxes[i].BackgroundImageLayout = ImageLayout.Stretch;
            }

            await Task.Delay(timeDelay);

            // Sao chép từ hàng 1 sang hàng 2 và xóa hàng 1
            CopyRowImages(firstRowBoxes, secondRowBoxes);
            ClearRow(firstRowBoxes);

            await Task.Delay(timeDelay);

            var isrCrash = false;

            // Sao chép từ hàng 2 sang hàng 3 và kiểm tra va chạm
            for (int i = 0; i < secondRowBoxes.Count; i++)
            {
                if (secondRowBoxes[i].BackgroundImage != null)
                {
                    if (i == playerCarPosition)
                    {
                        // Nếu vị trí trùng với xe người chơi, đổi hình ảnh thành crashCars.png
                        string crashCarPath = GetCarImagePath("crashCars.png");
                        thirdRowBoxes[i].BackgroundImage = Image.FromFile(crashCarPath);
                        thirdRowBoxes[i].BackgroundImageLayout = ImageLayout.Stretch;
                        PauseGame();
                        readyBtn.Enabled = true;
                        isrCrash = true;
                    }
                    else
                    {
                        // Sao chép hình ảnh từ hàng 2 sang hàng 3
                        thirdRowBoxes[i].BackgroundImage = secondRowBoxes[i].BackgroundImage;
                        thirdRowBoxes[i].BackgroundImageLayout = ImageLayout.Stretch;
                    }
                }
            }

            // Cập nhật điểm số sau mỗi lần vượt qua chướng ngại vật
            if(isrCrash){
                countScore = 0;
                currentScore.Text = "0";
            }
            else
            {
                countScore++;
                currentScore.Text = countScore.ToString();
            }

            // Xóa hình ảnh trong các PictureBox của hàng 2 sau khi sao chép xong
            ClearRow(secondRowBoxes);
        }



        private void CopyRowImages(List<PictureBox> source, List<PictureBox> target)
        {
            for (int i = 0; i < source.Count; i++)
            {
                if (source[i].BackgroundImage != null)
                {
                    target[i].BackgroundImage = source[i].BackgroundImage;
                    target[i].BackgroundImageLayout = ImageLayout.Stretch;
                }
            }
        }

        private void ClearRow(List<PictureBox> row)
        {
            string playerCarImageName = "playerCar.png"; // Tên ảnh đại diện cho xe của người chơi

            foreach (var box in row)
            {
                // Chỉ clear nếu hình ảnh không phải của playerCar
                if (box.Tag == null || box.Tag.ToString() != playerCarImageName)
                {
                    box.BackgroundImage = null;
                    box.Tag = null;
                }
            }
        }


        private string GetCarImagePath(string carImage)
        {
            string baseDirectory = AppDomain.CurrentDomain.BaseDirectory;
            return Path.Combine(baseDirectory, "..", "..", "imgs", "cars", carImage);
        }

        private void StartGame()
        {
            if (timer == null)
            {
                int timeDelay = 2300;

                // Điều chỉnh timeDelay tùy theo điểm số
                timeDelay -= (countScore / 4) * 400;  // Mỗi 3 điểm giảm 300ms

                timer = new Timer { Interval = timeDelay };
                timer.Tick += Timer_Tick;
            }

            if (!isEnd)
            {
                timer.Start();
            }
        }

        private void PauseGame()
        {
            if (timer != null)
            {
                timer.Stop();
                isEnd = true;
                if(countScore > highestScoreInt)
                {
                    highestScore.Text = countScore.ToString();
                    highestScoreInt = countScore;
                }
                countScore = 0;
                currentScore.Text = "0";
                goLeftBtn.Enabled = false;
                goRightBtn.Enabled = false;
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            ChangeRandomPictureBoxBackground();
        }

        private async Task CountDownAsync()
        {
            titleReady.Visible = true;
            titleReady.Text = "READY?";
            await Task.Delay(300);

            titleReady.Text = "3";
            await Task.Delay(500);

            titleReady.Text = "2";
            await Task.Delay(500);

            titleReady.Text = "1";
            await Task.Delay(500);

            titleReady.Text = "GO!!";
            await Task.Delay(1000);

            titleReady.Visible = false; // Ẩn sau khi đếm xong
        }


        private void readyBtn_Click(object sender, EventArgs e)
        {
            CountDownAsync();

            goLeftBtn.Enabled = true;
            goRightBtn.Enabled = true;

            isEnd = false;
            StartGame();
            readyBtn.Enabled = false;
            ClearRow(thirdRowBoxes);
            ClearRow(secondRowBoxes);
            ClearRow(firstRowBoxes);
            InitializePlayerCar();
            this.Focus();
        }

        private void goLeftBtn_Click(object sender, EventArgs e)
        {
            MoveCarLeft();
        }

        private void goRightBtn_Click(object sender, EventArgs e)
        {
            MoveCarRight();
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var result = MessageBox.Show("Are you sure you want to exit?", "Confirm Exit", MessageBoxButtons.YesNo);
            if (result == DialogResult.Yes)
            {
                this.Close();
            }
        }
    }
}
