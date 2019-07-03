﻿/*
 * Copyright (C) 2015 by Mike Bos
 *
 * This program is free software; you can redistribute it and/or modify it under the terms of the GNU General Public License as published by the Free Software Foundation;
 * either version 3 of the License, or any later version.
 *
 * This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY; without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR
 * PURPOSE. See the GNU General Public License for more details.
 *
 * A copy of the license is obtainable at http://www.gnu.org/licenses/gpl-3.0.en.html#content
*/

namespace Pass4Win
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Windows.Forms;
    using GpgApi;
    using LibGit2Sharp;

    /// <summary>
    /// The form Key manager
    /// </summary>
    public partial class FrmKeyManager : Form
    {
        private readonly FileSystemInterface _fileSystemInterface;
        private readonly ConfigHandling _config;
        private readonly KeySelect _keySelect;

        /// <summary>
        /// Initializes a new instance of the <see cref="FrmKeyManager"/> class.
        /// </summary>
        public FrmKeyManager(FileSystemInterface fileSystemInterface, ConfigHandling config, KeySelect keySelect)
        {
            _fileSystemInterface = fileSystemInterface;
            _config = config;
            _keySelect = keySelect;
            InitializeComponent();
            GpgInterface.ExePath = _config["GPGEXE"];

            ListDirectory(treeView1);
        }

        /// <summary>
        /// Filles the treenode.
        /// </summary>
        /// <param name="treeView">Which treeview to fill</param>
        private void ListDirectory(TreeView treeView)
        {
            // Get the TreeView ready for node creation.
            treeView.BeginUpdate();
            treeView.Nodes.Clear();
            TreeNode[] nodes = _fileSystemInterface.UpdateDirectoryTree();
            treeView.Nodes.AddRange(nodes);

            // Notify the TreeView to resume painting.
            treeView.EndUpdate();
        }

        /// <summary>
        /// Shows the gpg keys for that specific directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void TreeView1AfterSelect(object sender, TreeViewEventArgs e)
        {
            string tmpFile = treeView1.SelectedNode.Tag + "\\.gpg-id";
            if (File.Exists(tmpFile))
            {
                listBox1.Items.Clear();
                using (StreamReader r = new StreamReader(tmpFile))
                {
                    string line;
                    while ((line = r.ReadLine()) != null)
                    {
                        listBox1.Items.Add(line);
                    }
                }

                listBox1.SelectedIndex = 0;
            }
            else
            {
                listBox1.Items.Clear();
                listBox1.Items.Add(Strings.Error_keys_set);
            }
        }

        /// <summary>
        /// Adds a key to a selected directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void AddToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (_keySelect.ShowDialog() == DialogResult.OK)
            {
                if (listBox1.Items[0].ToString() == Strings.Error_keys_set)
                {
                    listBox1.Items.Clear();
                }

                listBox1.Items.Add(_keySelect.Gpgkey);
                string tmpFile = Path.GetDirectoryName(_config["PassDirectory"]) + "\\" +
                                 treeView1.SelectedNode.FullPath + "\\.gpg-id";
                using (StreamWriter w = new StreamWriter(tmpFile))
                {
                    foreach (var line in listBox1.Items)
                    {
                        w.WriteLine(line.ToString());
                    }
                }

                DirectoryInfo path = new DirectoryInfo(Path.GetDirectoryName(_config["PassDirectory"]) + "\\" +
                                                       treeView1.SelectedNode.FullPath);

                foreach (var ffile in path.GetFiles())
                {
                    if (!ffile.Name.StartsWith("."))
                        Recrypt(ffile.FullName);
                }

                ScanDirectory(path);
            }

            _keySelect.Close();
        }

        /// <summary>
        /// Ensures the files in a given directory are encrypted with all the current keys.
        /// </summary>
        /// <param name="path">DirectoyInfo</param>
        private void ScanDirectory(DirectoryInfo path)
        {
            foreach (var directory in path.GetDirectories())
            {
                if (!File.Exists(directory.FullName + "\\" + ".gpg-id"))
                {
                    foreach (var ffile in directory.GetFiles().Where(ffile => !ffile.Name.StartsWith(".")))
                    {
                        this.Recrypt(ffile.FullName);
                    }
                }

                if (!directory.Name.StartsWith("."))
                {
                    ScanDirectory(directory);
                }
            }
        }

        /// <summary>
        /// Remove a GPG key from a directory
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RemoveToolStripMenuItemClick(object sender, EventArgs e)
        {
            if (listBox1.Items[0].ToString() != Strings.Error_keys_set)
                if (listBox1.Items.Count > 1)
                {
                    listBox1.Items.Remove(listBox1.SelectedItem);
                    listBox1.Refresh();
                    string tmpFile = Path.GetDirectoryName(_config["PassDirectory"]) + "\\" +
                                     treeView1.SelectedNode.FullPath + "\\.gpg-id";
                    File.Delete(tmpFile);
                    using (StreamWriter w = new StreamWriter(tmpFile))
                    {
                        foreach (var line in listBox1.Items)
                        {
                            w.WriteLine(line.ToString());
                        }
                    }

                    using (var repo = new Repository(_config["PassDirectory"]))
                    {
                        Commands.Stage(repo, tmpFile);
                        repo.Commit("gpgid changed", new Signature("pass4win", "pass4win", DateTimeOffset.Now),
                            new Signature("pass4win", "pass4win", DateTimeOffset.Now));
                    }
                }

            DirectoryInfo path = new DirectoryInfo(Path.GetDirectoryName(_config["PassDirectory"]) + "\\" +
                                                   treeView1.SelectedNode.FullPath);

            foreach (var ffile in path.GetFiles().Where(ffile => !ffile.Name.StartsWith(".")))
            {
                this.Recrypt(ffile.FullName);
            }

            ScanDirectory(path);
        }

        /// <summary>
        /// Fires a decrypt thread with a callback the encrypts it again, used to make the keys current
        /// </summary>
        /// <param name="path"></param>
        private void Recrypt(string path)
        {
            string tmpFile = Path.GetTempFileName();
            GpgDecrypt decrypt = new GpgDecrypt(path, tmpFile);
            {
                // The current thread is blocked until the decryption is finished.
                GpgInterfaceResult result = decrypt.Execute();
                this.DecryptCallback(result, tmpFile, path);
            }
        }

        /// <summary>
        /// Callback from recrypt, ensures the file is encrypted again with the current keys
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tmpFile"></param>
        /// <param name="path"></param>
        private void DecryptCallback(GpgInterfaceResult result, string tmpFile, string path)
        {
            if (result.Status == GpgInterfaceStatus.Success)
            {
                List<KeyId> recipients = new List<KeyId>();
                foreach (var line in listBox1.Items)
                {
                    GpgListSecretKeys publicKeys = new GpgListSecretKeys();
                    publicKeys.Execute();
                    recipients.AddRange(from key in publicKeys.Keys
                        where key.UserInfos[0].Email == line.ToString()
                        select key.Id);
                }


                string tmpFile2 = Path.GetTempFileName();
                GpgEncrypt encrypt = new GpgEncrypt(tmpFile, tmpFile2, false, false, null, recipients,
                    CipherAlgorithm.None);
                GpgInterfaceResult encResult = encrypt.Execute();
                this.EncryptCallback(encResult, tmpFile, tmpFile2, path);
            }
            else
            {
                // shit happened
            }
        }

        /// <summary>
        /// Callback from Decrypt_Callback, ensures the tmp files are deleted and the file get's the correct name
        /// </summary>
        /// <param name="result"></param>
        /// <param name="tmpFile"></param>
        /// <param name="tmpFile2"></param>
        /// <param name="path"></param>
        public void EncryptCallback(GpgInterfaceResult result, string tmpFile, string tmpFile2, string path)
        {
            if (result.Status == GpgInterfaceStatus.Success)
            {
                File.Delete(tmpFile);
                File.Delete(path);
                File.Move(tmpFile2, path);
            }
        }
    }
}