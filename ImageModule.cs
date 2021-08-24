public class ImageModule : ModuleBase<SocketCommandContext>
    {
        public IEnumerable<string> WholeChunks(string str, int chunkSize)
        {
            for (int i = 0; i < str.Length; i += chunkSize)
                yield return str.Substring(i, chunkSize);
        }
        public struct HSVColor
        {
            public double Hue;
            public double Saturation;
            public double Value;
        }
        public static HSVColor GetHSV(Colour color)
        {
            HSVColor toReturn = new HSVColor();

            int max = Math.Max(color.R, Math.Max(color.G, color.B));
            int min = Math.Min(color.R, Math.Min(color.G, color.B));

            toReturn.Hue = Math.Round(color.GetHue(), 2);
            toReturn.Saturation = ((max == 0) ? 0 : 1d - (1d * min / max)) * 100;
            toReturn.Saturation = Math.Round(toReturn.Saturation, 2);
            toReturn.Value = Math.Round(((max / 255d) * 100), 2);

            return toReturn;
        }
        public static bool IsBetween(double x, double lower, double upper)
        {
            return lower <= x && x <= upper;
        }
        public static Bitmap ByteArrayToImage(byte[] source)
        {
            using (var ms = new MemoryStream(source))
            {
                return new Bitmap(ms);
            }
        }
        //https://stackoverflow.com/questions/52271491/convert-byte-to-bitmap-in-core-2-1

        [Command("compress")]
        [Summary("Compresses an image into emoji squares")]
        public async Task CompressAsync(
            [Summary("Compresses an image into emoji squares")]int maxWidth=30)
        {
            var attachments = Context.Message.Attachments;
            //Create a new WebClient instance.
            WebClient myWebClient = new WebClient();
            string url = attachments.ElementAt(0).Url;
            //Download the resource and load the bytes into a buffer.
            byte[] buffer = myWebClient.DownloadData(url);
            Bitmap img = ByteArrayToImage(buffer);
            string message = "`\n`";
            int newWidth = maxWidth;
            //Discord messages allow a maximum of 2000 characters per message, the following calculates the maximum resolution that maintains
            while ((newWidth * newWidth * (img.Width / img.Height)) + ((newWidth * (img.Width / img.Height)) * 2) > 2000){
                newWidth -= 1;
            }
            Console.WriteLine("Image has Width {0}", img.Width);
            Console.WriteLine("Image has Height {0}", img.Height);
            Console.WriteLine("Image has aspect ratioo of {0}", (double)img.Width/img.Height);
            Console.WriteLine("Using resolution of {0}", newWidth);
            //the image is split into blocks that have their rgb values compressed into a single value that then construct the compressed image
            double blocksize = (double)img.Width / newWidth;
            //i and j are the coordinate values of the compressed image, such that a change of n in one is equal to a change in position of the original image by n*blocksize pixels
            for (int i = 0; (int)(i * blocksize) < img.Height; i++)
            {
                for (int j = 0; j < newWidth; j++)
                {
                    int[] coloursome = new int[3];
                    int count = 0;
                    //k and l are the local coordinates inside the current block
                    for (int k = 0; k <= (int)blocksize; k += 1)
                    {
                        for (int l = 0; l <= (int)blocksize; l += 1)
                        {
                            if ((int)(i * blocksize) + k >= img.Height || (int)(j * blocksize) + l >= img.Width) 
                            {
                                //OoB check
                                continue;
                            }
                            //saves the squared colour values in the block, to be averaged later
                            Colour rgb = img.GetPixel((int)(j * blocksize + l), (int)(i * blocksize) + k);
                            coloursome[0] += (int)Math.Pow(rgb.R, 2);
                            coloursome[1] += (int)Math.Pow(rgb.G, 2);
                            coloursome[2] += (int)Math.Pow(rgb.B, 2);
                            count++;
                        }
                    }
                    if (count == 0) count = 1;
                    Colour p = Colour.FromArgb((int) Math.Sqrt(coloursome[0]/count),
                        (int)Math.Sqrt(coloursome[1] / count),
                        (int)Math.Sqrt(coloursome[2] / count));
                    //convert to HSV for easier colour delineation
                    HSVColor pixel = GetHSV(p);
                    if (IsBetween(pixel.Saturation, 0, 0.2*100))
                    {
                        if (IsBetween(pixel.Value, 0.7 * 100, 1.0 * 100))
                        {
                            message = message.Insert(message.Length - 1,"⬜"); //white_square
                        }
                        else
                        {
                            message = message.Insert(message.Length - 1,"⬛"); //black_square
                        }
                    }
                    else if (IsBetween(pixel.Value, 0, 0.2*100))
                    {
                        message = message.Insert(message.Length - 1,"⬛"); //black_square
                    }
                    else if (IsBetween(pixel.Hue, 0, 0.04*359))    //red
                    {
                        if (IsBetween(pixel.Saturation, 0.4 * 100, 1.0 * 100))
                        {
                            message = message.Insert(message.Length - 1,"🟥");  //red_square
                        }
                        else
                        {
                            message = message.Insert(message.Length - 1,"🟪"); //purple_square
                        }
                    }
                    else if (IsBetween(pixel.Hue, 0.04 * 359, 0.125*359)) //orange
                    {
                        if (IsBetween(pixel.Value, 0.2 * 100, 0.5*100))
                        {
                            message = message.Insert(message.Length - 1,"🟫");  //brown_square
                        }
                        else
                        {
                            message = message.Insert(message.Length - 1,"🟧"); //orange_square
                        }
                    }
                    else if (IsBetween(pixel.Hue, 0.125 * 359, 0.18*359))    //yellow
                    {
                        message = message.Insert(message.Length - 1,"🟨");   //yellow_square
                    }
                    else if (IsBetween(pixel.Hue, 0.18 * 359, 0.45*359)) //green
                    {
                        message = message.Insert(message.Length - 1,"🟩"); //green_square
                    }
                    else if (IsBetween(pixel.Hue, 0.45 * 359, 0.73*359)) //blue
                    {
                        message = message.Insert(message.Length - 1,"🟦"); //blue_square
                    }
                    else if (IsBetween(pixel.Hue, 0.73 * 359, 0.9*359))  //purple
                    {
                        message = message.Insert(message.Length - 1,"🟪");   //purple_square
                    }
                    else//(IsBetween(pixel.Hue, 0.9 * 359, 1.0*360))   //red
                    {
                        if (IsBetween(pixel.Saturation, 0.4 * 100, 1.0 * 100))
                        {
                            message = message.Insert(message.Length - 1,"🟥"); //red_square
                        }
                        else
                        {
                            message = message.Insert(message.Length - 1,"🟪");   //purple_square
                        }
                    }
                }
                message = message.Insert(message.Length - 1,"\n");
                Console.WriteLine("Message length: {0}", message.Length);
            }
            // Function used to recompress image smaller until it met length limit, was inefficient but kept the check to handle unforseen inaccuracies 
            if (message.Length > 2000)
            {
                await CompressAsync(maxWidth - 1);
            }
            else 
            {
                Console.WriteLine("Download successful.");
                await ReplyAsync(message);
            }
        }
    }
