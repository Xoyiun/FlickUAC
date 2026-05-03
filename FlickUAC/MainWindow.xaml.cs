using IWshRuntimeLibrary;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace FlickUAC
{
    public partial class MainWindow : Window
    {
        private static readonly string registryPath = @"Software\Microsoft\Windows NT\CurrentVersion\AppCompatFlags\Layers";
        string[] supportedLanguages = { "en-US", "zh-CN", "zh-TW" };
        string[] autoSeachProcessName = {
        "Endfield",      // 明日方舟：終末地
        "GenshinImpact", // 原神
        "YuanShen",      // 原神 (陸版)
        "StarRail",      // 崩壞：星穹鐵道
        "ZenlessZoneZero", // 絕區零
        "Wuthering Waves", // 鳴潮
        "Project_Mugen",  // 代號：無限
        "BH3",           // 崩壞 3rd
        "Naraka"         // 永劫無間
        };
        private const uint PROCESS_QUERY_LIMITED_INFORMATION = 0x1000;
        [DllImport("kernel32.dll", SetLastError = true, CharSet = CharSet.Unicode)]

        private static extern bool QueryFullProcessImageName([In] IntPtr hProcess, [In] int dwFlags, [Out] StringBuilder lpExeName, [In, Out] ref int lpdwSize);
        [DllImport("kernel32.dll", SetLastError = true)]

        private static extern IntPtr OpenProcess(uint processAccess, bool bInheritHandle, int processId);
        [DllImport("kernel32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]

        private static extern bool CloseHandle(IntPtr hObject);

        public class UacItem : INotifyPropertyChanged
        {
            public required string ItemPath { get; set; }
            public required ImageSource Icon { get; set; }

            public event PropertyChangedEventHandler? PropertyChanged;
            protected void OnPropertyChanged([CallerMemberName] string? name = null)
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
            }
        }
        public class LanguageItem
        {
            public required string Code { get; set; }
            public required string NativeName { get; set; }
        }
        public ObservableCollection<UacItem> DisplayItems { get; set; } = new();
        
        public class languageResource
        {
            public static languageResource Current { get; } = new languageResource();
            public string Error => Get("error");
            public string Message => Get("message");
            public string NoActionTaken => Get("noActionTaken");
            public string ThisFeatureRequiresAdministratorPrivileges => Get("thisFeatureRequiresAdministratorPrivileges");
            public string ReFlash => Get("ReFlash");
            public string RegistryValueAdded => Get("registryValueAdded");
            public string RegistryValueDeleted => Get("registryValueDeleted");
            public string TheSelectedFilePathIs => Get("theSelectedFilePathIs");
            public string WhetherToDeleteTheValue => Get("whetherToDeleteTheValue");
            private string Get(string key)
            {
                return System.Windows.Application.Current.TryFindResource(key)?.ToString() ?? $"[{key}]";
            }
        }
    
        private void AddItemToRegistry(string selectedFilePath)
        {
            MessageBoxResult result = MessageBox.Show(
                        $"{languageResource.Current.TheSelectedFilePathIs}\n{selectedFilePath}",
                        languageResource.Current.Message,
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    using (var key = Microsoft.Win32.Registry.CurrentUser.CreateSubKey(registryPath))
                    {
                        key.SetValue(selectedFilePath, "RunAsInvoker");
                        MessageBox.Show(
                            languageResource.Current.RegistryValueAdded,
                            languageResource.Current.Message,
                            MessageBoxButton.OK,
                            MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, languageResource.Current.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
            else
            {
                MessageBox.Show(languageResource.Current.NoActionTaken, languageResource.Current.Message, MessageBoxButton.OK, MessageBoxImage.Information);
            }
            ReFlashItemPath();
        }
   
        private void ChangeLanguage(string languageCode)
        {
            string uriPath = $"/Resource/Language/{languageCode}.xaml";
            Uri uri = new Uri(uriPath, UriKind.Relative);
            try
            {
                ResourceDictionary newDict = new ResourceDictionary { Source = uri };
                var mergedDicts = Application.Current.Resources.MergedDictionaries;
                for (int i = 0; i < mergedDicts.Count; i++)
                {
                    if (mergedDicts[i].Source != null && mergedDicts[i].Source.OriginalString.Contains("/Language/"))
                    {
                        mergedDicts.RemoveAt(i);
                        break;
                    }
                }
                mergedDicts.Add(newDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Loading language resource failed: {ex.Message}");
            }
        }

        private string GetExecutablePath(int processId)
        {
            StringBuilder buffer = new StringBuilder(1024);
            int size = buffer.Capacity;
            IntPtr hProcess = OpenProcess(PROCESS_QUERY_LIMITED_INFORMATION, false, processId);
            if (hProcess != IntPtr.Zero)
            {
                try
                {
                    if (QueryFullProcessImageName(hProcess, 0, buffer, ref size))
                    {
                        return buffer.ToString();
                    }
                }
                finally
                {
                    CloseHandle(hProcess);
                }
            }
            return null;
        }
 
        private bool ReFlashItemPath()
        {
            bool hasValue = false;
            DisplayItems.Clear();
            try
            {
                using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath))
                {
                    if (key != null)
                    {
                        foreach (string valueName in key.GetValueNames())
                        {
                            if (key.GetValue(valueName)?.ToString()?.Contains("RunAsInvoker") == true)
                            {
                                hasValue = true;
                                ImageSource iconSource;
                                try
                                {
                                    using (var icon = System.Drawing.Icon.ExtractAssociatedIcon(valueName))
                                    {
                                        iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                            icon!.Handle, System.Windows.Int32Rect.Empty,
                                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                                    }
                                }
                                catch
                                {
                                    using (var defaultIcon = System.Drawing.SystemIcons.Application)
                                    {
                                        iconSource = System.Windows.Interop.Imaging.CreateBitmapSourceFromHIcon(
                                            defaultIcon.Handle, System.Windows.Int32Rect.Empty,
                                            System.Windows.Media.Imaging.BitmapSizeOptions.FromEmptyOptions());
                                    }
                                }
                                DisplayItems.Add(new UacItem { ItemPath = valueName, Icon = iconSource });
                            }
                        }
                    }
                }
                ItemList.ItemsSource = DisplayItems;
                ItemList.UnselectAll();
            }
            catch (Exception ex)
            {
                string errorTitle = languageResource.Current.Error;
                MessageBox.Show(ex.Message, errorTitle, MessageBoxButton.OK, MessageBoxImage.Error);
            }
            return hasValue;
        }

        public MainWindow()
        {
            InitializeComponent();
            var languages = supportedLanguages.Select(code =>
            {
                string nativeName;
                    try
                    {
                        nativeName = new CultureInfo(code).NativeName;
                    }
                    catch
                    {
                        nativeName = code;
                    }
                return new LanguageItem { Code = code, NativeName = nativeName };
            }).ToList();
            LanguageMenu.ItemsSource = languages;
            LanguageMenu.DisplayMemberPath = "NativeName";
            LanguageMenu.SelectedValuePath = "Code";
            string sysLang = CultureInfo.CurrentUICulture.Name;
            if (supportedLanguages.Contains(sysLang))
            {
                LanguageMenu.SelectedValue = sysLang;
            }
            else
            {
                LanguageMenu.SelectedIndex = 0;
            }

            ReFlashItemPath();
        }

        private void AutoSearch_Click(object sender, RoutedEventArgs e)
        {
    if (!new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator))
            {
                try
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo();
                    startInfo.UseShellExecute = true;
                    startInfo.WorkingDirectory = Environment.CurrentDirectory;
                    startInfo.FileName = Process.GetCurrentProcess().MainModule.FileName;
                    startInfo.Verb = "runas";
                    Process.Start(startInfo);
                    System.Windows.Application.Current.Shutdown();
                    return;
                }
                catch
                {
                    MessageBox.Show(languageResource.Current.ThisFeatureRequiresAdministratorPrivileges, languageResource.Current.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }

            try
            {
                foreach (var processName in autoSeachProcessName)
                {
                    Process[] processes = Process.GetProcessesByName(processName);

                    foreach (var item in processes)
                    {
                        try
                        {
                            string path = GetExecutablePath(item.Id);
                            if (!string.IsNullOrEmpty(path))
                            {
                                AddItemToRegistry(path);
                            }
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"{item.ProcessName}:\n{ex.Message}", languageResource.Current.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                        finally
                        {
                            item.Dispose();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, languageResource.Current.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ItemAdd_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new Microsoft.Win32.OpenFileDialog
                {
                    Filter = "Executables (*.exe)|*.exe|All files (*.*)|*.*",
                };
                if (openFileDialog.ShowDialog() == true)
                {
                    string selectedFilePath = openFileDialog.FileName;
                    AddItemToRegistry(selectedFilePath);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message, languageResource.Current.Message, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ItemDelete_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ItemList.SelectedItems.Cast<UacItem>().ToList();

            if (selectedItems.Count == 0) return;
            foreach (var item in selectedItems)
            {
                string message = $"{languageResource.Current.WhetherToDeleteTheValue}\n{item.ItemPath}";
                if (MessageBox.Show(message, languageResource.Current.Message, MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        using (RegistryKey? key = Registry.CurrentUser.OpenSubKey(registryPath, true))
                        {
                            if (key == null) return;
                            key.DeleteValue(item.ItemPath, false);
                            DisplayItems.Remove(item);
                        }
                        MessageBox.Show(languageResource.Current.RegistryValueDeleted, languageResource.Current.Message, MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(ex.Message, languageResource.Current.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void ItemList_SelectedCellsChanged(object sender, SelectedCellsChangedEventArgs e)
        {
            int selectedCount = ItemList.SelectedItems.Count;
            ItemDelete.IsEnabled = selectedCount > 0;
            ItemLocation.IsEnabled = selectedCount > 0;
        }

        private void ItemLocation_Click(object sender, RoutedEventArgs e)
        {
            var selectedItems = ItemList.SelectedItems.Cast<UacItem>().ToList();
            foreach (var item in selectedItems)
            {

                if (System.IO.File.Exists(item.ItemPath))
                {
                    Process.Start("explorer.exe", $"/select,\"{item.ItemPath}\"");
                }
                else
                {
                    string? directory = item.ItemPath;
                    do
                    {
                        if (Directory.Exists(directory))
                        {
                            Process.Start("explorer.exe", $"\"{directory}\"");
                            break;
                        }
                        directory = System.IO.Path.GetDirectoryName(directory);
                    } while (!System.IO.File.Exists(directory));
                }
            }
        }

        private void LanguageMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (LanguageMenu.SelectedValue is string selectedCode)
            {
                ChangeLanguage(selectedCode);
            }
        }

        private async void Window_DragOver(object sender, DragEventArgs e)
        {
            e.Effects = DragDropEffects.None;
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                if (files.All(f => System.IO.Path.GetExtension(f).Equals(".exe", StringComparison.OrdinalIgnoreCase) || System.IO.Path.GetExtension(f).Equals(".lnk", StringComparison.OrdinalIgnoreCase)))
                    e.Effects = DragDropEffects.Copy;
            }
            e.Handled = true;
        }

        private async void Window_Drop(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop))
            {
                string[] files = (string[])e.Data.GetData(DataFormats.FileDrop);
                foreach (string file in files)
                {
                    if (System.IO.Path.GetExtension(file).Equals(".lnk", StringComparison.OrdinalIgnoreCase))
                    {
                        try
                        {
                            AddItemToRegistry(((IWshShortcut)new WshShell().CreateShortcut(file)).TargetPath);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(ex.Message, languageResource.Current.Error, MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                    else
                        AddItemToRegistry(file);
                }
            }
        }
    }
}