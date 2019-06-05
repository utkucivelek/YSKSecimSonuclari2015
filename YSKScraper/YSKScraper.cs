using System.Threading;
using OpenQA.Selenium;
using OpenQA.Selenium.Firefox;
using OpenQA.Selenium.Support.UI;

namespace YSKScraper
{
    public class YSKScraper
    {
        public static void Scrape2015()
        {
            FirefoxDriver driver = new FirefoxDriver();

            driver.Navigate().GoToUrl("https://sonuc.ysk.gov.tr/module/GirisEkrani.jsf");

            // uyari kapat
            driver.FindElement(By.Name("closeMessageButton")).Click();

            // 2015'i sec
            driver.FindElement(By.Id("j_id111:secimSorgulamaForm:j_id114:secimSecmeTable:0:secimId:0")).Click();

            // tamam butonu
            driver.FindElement(By.Id("j_id111:secimSorgulamaForm:j_id141")).Click();

            // bekle
            Thread.Sleep(2000);

            //il combo
            var ilCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id52:j_id124:cmbSecimCevresi")));
            int ilCount = ilCombo.Options.Count;

            for (int i = 0; i < ilCount; i++)
            {
                ilCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id52:j_id124:cmbSecimCevresi")));
                ilCombo.SelectByIndex(i);
                Thread.Sleep(2000);

                //ilce combo
                var ilceCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id52:j_id170:cmbIlceSecimKurulu")));
                int ilceCount = ilceCombo.Options.Count;

                for (int j = 1; j < ilceCount; j++) // ilk ilce 1. sirada
                {
                    ilceCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id52:j_id170:cmbIlceSecimKurulu")));
                    ilceCombo.SelectByIndex(j);
                    Thread.Sleep(1000);

                    //sorgula button
                    driver.FindElement(By.Id("j_id48:j_id52:j_id195")).Click();
                    Thread.Sleep(5000);

                    // kaydet buton
                    driver.FindElement(By.Id("j_id48:tabloBilgileriPanel:j_id443")).Click();
                    Thread.Sleep(2000);

                    // kabul link
                    driver.FindElement(By.Id("j_id1119:j_id1120:j_id1126")).Click();

                    // cikan ilk download penceresinde, xls dosyalarini her zaman otomatik download et demek gerekiyor.

                    // save bekle
                    Thread.Sleep(8000);
                }
            }
        }

        public static void Scrape2011()
        {
            FirefoxDriver driver = new FirefoxDriver();
            driver.Navigate().GoToUrl("https://sonuc.ysk.gov.tr/module/GirisEkrani.jsf");

            // uyari kapat
            driver.FindElement(By.Name("closeMessageButton")).Click();

            // 2011'i sec
            driver.FindElement(By.Id("j_id111:secimSorgulamaForm:j_id114:secimSecmeTable:3:secimId:0")).Click();

            // tamam butonu
            driver.FindElement(By.Id("j_id111:secimSorgulamaForm:j_id141")).Click();

            Thread.Sleep(2000);

            //il combo
            var ilCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id49:j_id81:cmbSecimCevresi")));
            int ilCount = ilCombo.Options.Count;

            for (int i = 0; i < ilCount; i++)
            {
                ilCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id49:j_id81:cmbSecimCevresi")));
                ilCombo.SelectByIndex(i);
                Thread.Sleep(2000);

                //ilce combo -- starts at index 1
                var ilceCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id49:j_id116:cmbIlceSecimKurulu")));
                int ilceCount = ilceCombo.Options.Count;

                for (int j = 1; j < ilceCount; j++)
                {
                    ilceCombo = new SelectElement(driver.FindElement(By.Id("j_id48:j_id49:j_id116:cmbIlceSecimKurulu")));
                    ilceCombo.SelectByIndex(j);
                    Thread.Sleep(1000);

                    //sorgula button
                    driver.FindElement(By.Id("j_id48:j_id49:j_id150")).Click();

                    Thread.Sleep(5000);

                    // kaydet buton
                    driver.FindElement(By.Id("j_id48:tabloBilgileriPanel:j_id392")).Click();
                    Thread.Sleep(2000);

                    // kabul link
                    driver.FindElement(By.Id("j_id935:j_id936:j_id942")).Click();

                    // cikan ilk download penceresinde, xls dosyalarini her zaman otomatik download et demek gerekiyor.

                    // save bekle
                    Thread.Sleep(8000);
                }
            }
        }
    }
}
