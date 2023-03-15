//#define Debug
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Reflection;

namespace AutoWifiConnect
{
    class Program
    {

        static void Main(string[] args)
        {
            Console.Title = "AutoWifiConnect v1.0 20220209";   
            string ssid = string.Empty;
            string[] cmd_result;
            string program_path = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);//取得程式所在目錄
            string setting_txt_name = program_path+"\\setting.txt";

            bool go_connect = false;
            const int error_count_out = 10;
            int error_count_out_count = 0;
            ProcessStartInfo cmd_info = new ProcessStartInfo("cmd.exe");
            cmd_info.UseShellExecute = false;
            cmd_info.RedirectStandardInput = true;
            cmd_info.RedirectStandardOutput = true;
            cmd_info.RedirectStandardError = true;
            cmd_info.CreateNoWindow = true;

            

            if (File.Exists(setting_txt_name))
            {
                using (StreamReader reader = new StreamReader(setting_txt_name))
                {
                    ssid = reader.ReadToEnd();
                }

                if (ssid != string.Empty)
                {
                    Console.WriteLine("******************************");
                    Console.WriteLine("Wifi Auto Connect Start.");
                    Console.WriteLine("SSID is " + ssid);
                    Console.WriteLine("******************************");
                    Console.WriteLine();
                    while (true)
                    {
                        go_connect = false;


#if Debug
                        Console.WriteLine("input[netsh wlan show interface]");
#endif
                        cmd_result = RunCmd("netsh wlan show interface", cmd_info);


#if Debug
                        Console.WriteLine("output:");
                        Console.WriteLine("[0]:");
                        Console.WriteLine(cmd_result[0]);
                        Console.WriteLine("[1]:");
                        Console.WriteLine(cmd_result[1]);
#endif

                        if (cmd_result[0] != string.Empty)
                        {
                            string[] split_new_line = cmd_result[0].Split(new string[] { "\r\n" }, StringSplitOptions.RemoveEmptyEntries);
                            foreach (string s in split_new_line)
                            {
                                if (s.Contains("State"))
                                {
#if Debug
                                    Console.WriteLine("have[State]");
#endif
                                    string[] split_state = s.Split(':');
                                    if (split_state[1].Contains("disconnected"))
                                    {
                                        go_connect = true;
#if Debug
                                        Console.WriteLine("State[Disconnected]");
#endif
                                    }
                                    else
                                    {
#if Debug
                                        Console.WriteLine("State[connected]");
#endif
                                    }
                                    break;
                                }
#if Debug
                                else
                                {
                                    Console.WriteLine("no have[State]");
                                }

#endif
                            }
                        }
                        else
                        {
                            Show_Error_Msg("Error：\r\nCMD[netsh wlan show interface] is no output.");
                        }

                        if (cmd_result[1] != string.Empty)
                        {
                            Show_Error_Msg(cmd_result[1]);
                        }


                        if(go_connect)
                        {
#if Debug
                            Console.WriteLine($"input[netsh wlan connect {ssid}]");
#endif
                            cmd_result = RunCmd("netsh wlan connect " + ssid,cmd_info);

#if Debug
                            Console.WriteLine("output:");
                            Console.WriteLine("[0]:");
                            Console.WriteLine(cmd_result[0]);
                            Console.WriteLine("[1]:");
                            Console.WriteLine(cmd_result[1]);
#endif

                            if (cmd_result[1]!=string.Empty)
                            {
                                Show_Error_Msg(cmd_result[1]);
                            }
                        }

                        if (error_count_out_count > error_count_out)
                        {
                            Console.WriteLine("***Error count >10. Program Stop.***");
                            break;
                        }
                        SpinWait.SpinUntil(() => false, 10000);
                    }

                }
                else
                {
                    Console.WriteLine("***Setting.txt ssid is not exists.***");
                }


            }
            else
            {
                Console.WriteLine("***Setting.txt is not exists.***");
            }

            Console.WriteLine();
            Console.WriteLine("Key anything to close.");
            Console.ReadKey();


            string[] RunCmd(string cmd_str, ProcessStartInfo info)
            {
                string[] result = new string[2];
                using (Process p = new Process())
                {
                    p.StartInfo = info;
                    p.Start();
                    p.StandardInput.WriteLine(cmd_str);//netsh wlan show interface
                    p.StandardInput.Close();
                    result[0] = p.StandardOutput.ReadToEnd();
                    result[1] = p.StandardError.ReadToEnd();
                    p.WaitForExit();
                    p.Close();
                }

                return result;
            }

            void Show_Error_Msg(string str)
            {
                Console.WriteLine("**********************************************");
                Console.WriteLine(str);
                Console.WriteLine("**********************************************");
                error_count_out_count++;
            }
            
        }


    }
}
