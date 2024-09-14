using System;
using System.IO;
using System.Text;

namespace AppendModuleToJPG
{
    internal class Program
    {
        static void Main(string[] args)
        {
            if (args.Length < 3)
            {
                Console.WriteLine("Usage: AppendModuleToJPG <dotNetModulePath> <jpegImagePath> <outputImagePath>");
                return;
            }

            string modulePath = args[0];
            string imagePath = args[1];
            string outputPath = args[2];

            try
            {
                // Read the .NET module and convert to base64
                byte[] moduleBytes = File.ReadAllBytes(modulePath);
                string base64Module = Convert.ToBase64String(moduleBytes);
                Console.WriteLine("The .NET module has been successfully read and converted to base64.");
                //Console.WriteLine("The base64 module is: " + base64Module);

                // Read the original image
                byte[] imageBytes = File.ReadAllBytes(imagePath);

                string flag = "Redemption";
                byte[] flagBytes = Encoding.ASCII.GetBytes(flag);

                string end_flag = "EndRedemption";
                byte[] end_flagBytes = Encoding.ASCII.GetBytes(end_flag);

                // Open file stream for output
                using (FileStream output = new FileStream(outputPath, FileMode.Create, FileAccess.Write))
                {
                    // Write the original image data
                    output.Write(imageBytes, 0, imageBytes.Length);

                    //Add flag to the image
                    output.Write(flagBytes, 0, flagBytes.Length);

                    // Encode the base64 module data as ASCII and write it
                    byte[] base64Bytes = Encoding.ASCII.GetBytes(base64Module);
                    output.Write(base64Bytes, 0, base64Bytes.Length);

                    //Add end flag to the image
                    output.Write(end_flagBytes, 0, end_flagBytes.Length);
                }

                Console.WriteLine("The .NET module has been successfully embedded into the image.");
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error: " + ex.Message);
            }
        
        }
    }
}
