using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace PIGIBIG_PI_UPLOADER
{
    class Program
    {
        private static bool selfUpload = Properties.Settings.Default.SELF_UPLOAD;
        private static string dropSite = Properties.Settings.Default.DROP_SITE;
        private static string fileExtension = Properties.Settings.Default.FILE_EXT;
        private static string backup = Path.Combine(dropSite, "backup");
        private static List<string> invFiles = null;

        static void Main(string[] args)
        {
            try
            {
                string fileExt = string.Empty;

                invFiles = new List<string>();

                if (!Directory.Exists(dropSite))
                    Directory.CreateDirectory(dropSite);

                invFiles = Directory.GetFiles(dropSite, fileExtension).ToList();

                foreach (var item in invFiles)
                {
                    try
                    {
                        fileExt = Path.GetExtension(item);

                        ProcessFile(item);
                        Thread.Sleep(1000);

                        //Move processed fie to backup for reference
                        if (Directory.Exists(backup))
                            File.Move(item, Path.Combine(backup, Path.GetFileName(item)));
                        else
                        {
                            Directory.CreateDirectory(backup);
                            File.Move(item, Path.Combine(backup, Path.GetFileName(item)));
                        }
                    }
                    catch
                    {
                        throw;
                    }
                }

                if (selfUpload)
                    UploadFromPOS();

            }
            catch (Exception er)
            {
                var sb = new StringBuilder();
                sb.Append($"{DateTime.Now.ToString()} -> {er.Message} {Environment.NewLine}");

                File.AppendAllText(Path.Combine(dropSite, "Error.txt"), sb.ToString());
            }
        }

        static void ProcessFile(string files)
        {
            try
            {
                List<PigibigInvoice> inv = new List<PigibigInvoice>();
                PigibigInvoice pi = new PigibigInvoice();
                string fileExt = string.Empty;
                string[] data = new string[] { };

                fileExt = Path.GetExtension(files);

                if(fileExt.CompareTo(".csv") == 0)
                {
                    foreach (var item in File.ReadAllLines(files))
                    {
                        var test = item.Replace("\"", "");

                        data = test.Split(',');

                        inv.Add(new PigibigInvoice
                        {
                            Reference = data[0].ToString(),
                            SowNo = data[1].ToString(),
                            ParityNo = data[2].ToString(),
                            TypeTransaction = data[3].ToString(),
                            BatchNo = data[4].ToString(),
                            IdStock = data[5].ToString(),
                            Description = data[6].ToString(),
                            Category = data[7].ToString(),
                            Qty = data[8].ToString(),
                            UOM = data[9].ToString(),
                            Price = data[10].ToString(),
                            TotalAmount = data[11].ToString(),
                            InvoiceDate = data[12].ToString(),
                            CodeNo = data[13].ToString(),
                            AccountNo = data[14].ToString(),
                            BranchCode = data[15].ToString(),
                            BranchName = data[16].ToString()
                        });
                    }
                }
                else
                {
                    DataTable dt = new DataTable();

                    dt = pi.GetPIFromFile(files);

                    inv = dt.AsEnumerable()
                        .Select(x => new PigibigInvoice()
                        {
                            AccountNo = x["ACCOUNT_NUMBER"].ToString(),
                            BatchNo = x["BATCH"].ToString(),
                            BranchCode = x["BRANCH_CODE"].ToString(),
                            BranchName = x["BRANCH_NAME"].ToString(),
                            Category = x["CATEGORY"].ToString(),
                            CodeNo = x["CODE_NO"].ToString(),
                            Description = x["DESCRIPTION"].ToString(),
                            IdStock = x["ITEM_CODE"].ToString(),
                            InvoiceDate = Convert.ToDateTime(x["INV_DATE"]).ToString("yyyy-MM-dd"),
                            ParityNo = x["PARITY"].ToString(),
                            Price = (Convert.ToDecimal(x["PRICE"])).ToString(),
                            Qty = (Convert.ToDecimal(x["QUANTITY"])).ToString(),
                            Reference = x["REFERENCE"].ToString(),
                            SowNo = x["SOW"].ToString(),
                            TotalAmount = (Convert.ToDecimal(x["AMOUNT"])).ToString(),
                            TypeTransaction = x["TYPE"].ToString(),
                            UOM = x["UOM"].ToString()
                        }).ToList();
                }


                

                if (inv.Count > 0)
                    pi.SaveTransaction(inv);

            }
            catch
            {

                throw;
            }
        }

        static void UploadFromPOS()
        {
            try
            {
                List<PigibigInvoice> inv = new List<PigibigInvoice>();
                PigibigInvoice pi = new PigibigInvoice();

                string reference = pi.GetLatestPI();

                inv = pi.GetPI(reference);

                if(inv.Count > 0)
                {
                    //Save PI
                    pi.SaveTransaction(inv);

                    //Update POS extracted PI = 'Y'
                    //pi.UpdateExtractedPI();
                }

            }
            catch
            {

                throw;
            }
        }
        
    }
}
