using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data.SqlClient;
using System.Data;
using System.Configuration;

namespace lab2_KPO
{
    public partial class MainWindow : Window
    {
        SqlConnection connection;
        string connectionString;

        public MainWindow()
        {
            InitializeComponent();
            connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;
            connection = new SqlConnection();
            connection.ConnectionString = connectionString;
        }

        #region UI_events
        private void CreateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (DBtree.SelectedItem == TreeRoot || DBtree.SelectedItem == null)
                    AddUserTo(TreeRoot);
                else if (DBtree.SelectedItem is TreeViewItem)
                {
                    TreeViewItem treeViewItem = (TreeViewItem)DBtree.SelectedItem;

                    if (treeViewItem.Header is User)
                        AddCharacterTo(treeViewItem);
                    else if (treeViewItem.Header is Character)
                        AddItemTo(treeViewItem);
                    else
                        throw new Exception("???");
                }

                if (((TreeViewItem)DBtree.SelectedItem).IsExpanded)
                    ((TreeViewItem)DBtree.SelectedItem).IsExpanded = true;
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void UpdateButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                UpdateSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void DeleteButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                DeleteSelected();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void ExitButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void DBtree_Expanded(object sender, RoutedEventArgs e)
        {
            TreeViewItem treeViewItem = (TreeViewItem)e.Source;

            try
            {
                if (treeViewItem == TreeRoot)
                {
                    LoadUsersTo(treeViewItem);
                }
                if (treeViewItem.Header is User)
                {
                    LoadCharactersTo(treeViewItem);
                }
                else if (treeViewItem.Header is Character)
                {
                    LoadItemsTo(treeViewItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        private void DBtree_SelectedChanged(object sender, RoutedEventArgs e)
        {
            if (DBtree.SelectedItem == null)
            {
                MenuDelete.IsEnabled = false;
                MenuUpdate.IsEnabled = false;
                MenuCreate.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;
                CreateButton.IsEnabled = false;
            }
            else if (((TreeViewItem)DBtree.SelectedItem).Header is Item)
            {
                MenuDelete.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                MenuUpdate.IsEnabled = true;
                UpdateButton.IsEnabled = true;
                MenuCreate.IsEnabled = false;
                CreateButton.IsEnabled = false;
            }
            else if (DBtree.SelectedItem == TreeRoot)
            {
                MenuDelete.IsEnabled = false;
                MenuUpdate.IsEnabled = false;
                DeleteButton.IsEnabled = false;
                UpdateButton.IsEnabled = false;
                MenuCreate.IsEnabled = true;
                CreateButton.IsEnabled = true;
            }
            else
            {
                MenuDelete.IsEnabled = true;
                MenuUpdate.IsEnabled = true;
                MenuCreate.IsEnabled = true;
                DeleteButton.IsEnabled = true;
                UpdateButton.IsEnabled = true;
                CreateButton.IsEnabled = true;
            }
        }
        #endregion
        #region Add
        private void AddUserTo(TreeViewItem targetTreeItem)
        {
            User user = GetUserFromInput();

            if (user == null)
                return;

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
            keyValuePairs.Add("Name", user.name);

            if (DB_Insert("[User]", keyValuePairs))
                LoadUsersTo(targetTreeItem);
        }

        private void AddCharacterTo(TreeViewItem targetTreeItem)
        {
            Character character = GetCharacterFromInput();

            if (character == null)
                return;

            character.user = (User)targetTreeItem.Header;

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
            keyValuePairs.Add("Name", character.name);
            keyValuePairs.Add("User_Id", ((User)targetTreeItem.Header).id);
            keyValuePairs.Add("Level", character.level);

            if (character != null && DB_Insert("Character", keyValuePairs))
                LoadCharactersTo(targetTreeItem);
        }

        private void AddItemTo(TreeViewItem targetTreeItem)
        {
            Item item = GetItemFromInput();

            if (item == null)
                return;

            item.character = (Character)targetTreeItem.Header;

            Dictionary<string, object> keyValuePairs = new Dictionary<string, object>();
            keyValuePairs.Add("Name", item.name);
            keyValuePairs.Add("Character_Id", ((Character)targetTreeItem.Header).id);

            if (item != null && DB_Insert("Item", keyValuePairs))
                LoadItemsTo(targetTreeItem);
        }
        #endregion
        #region GetFromInput
        private User GetUserFromInput(User currentUser = null)
        {
            if (currentUser == null)
                currentUser = new User(0, "");

            MultipleInputFieldWindow inputWindow = new MultipleInputFieldWindow();
            inputWindow.AddInputField("name", "Имя пользователя:", currentUser.name);
            inputWindow.Width = 120;
            inputWindow.ShowInTaskbar = false;
            inputWindow.Left = this.Left + 30;
            inputWindow.Top = this.Top + 30;
            inputWindow.ShowDialog();

            if (inputWindow.DialogResult == true)
                return new User(currentUser.id, inputWindow.GetInputFieldValue("name"));
            else
                return null;
        }

        private Character GetCharacterFromInput(Character currentCharacter = null)
        {
            if (currentCharacter == null)
                currentCharacter = new Character(0, null, "");

            MultipleInputFieldWindow inputWindow = new MultipleInputFieldWindow();
            inputWindow.AddInputField("name", "Имя персонажа:", currentCharacter.name);
            inputWindow.AddInputField("level", "Уровень:", currentCharacter.level.ToString(),
                x => int.TryParse(x, out int val) ? val >= 0 : false);
            inputWindow.Width = 120;
            inputWindow.ShowInTaskbar = false;
            inputWindow.Left = this.Left + 30;
            inputWindow.Top = this.Top + 30;
            inputWindow.ShowDialog();

            if (inputWindow.DialogResult == true)
            {
                string level = inputWindow.GetInputFieldValue("level");
                if (level != null)
                    return new Character(currentCharacter.id, currentCharacter.user,
                        inputWindow.GetInputFieldValue("name"), int.Parse(level));
                else
                    throw new Exception("Уровень может быть только целым числом больше 0");

            }
            else
                return null;
        }

        private Item GetItemFromInput(Item currentItem = null)
        {
            if (currentItem == null)
                currentItem = new Item(0, null, "");

            MultipleInputFieldWindow inputWindow = new MultipleInputFieldWindow();
            inputWindow.AddInputField("name", "Название предмета:", currentItem.name);
            inputWindow.Width = 120;
            inputWindow.ShowInTaskbar = false;
            inputWindow.Left = this.Left + 30;
            inputWindow.Top = this.Top + 30;
            inputWindow.ShowDialog();

            if (inputWindow.DialogResult == true)
                return new Item(currentItem.id, currentItem.character, inputWindow.GetInputFieldValue("name"));
            else
                return null;
        }

        #endregion

        private void DeleteSelected()
        {
            if (DBtree.SelectedItem == null)
                throw new Exception("???");

            try
            {
                TreeViewItem treeViewItem = (TreeViewItem)DBtree.SelectedItem;
                string sqlCommand = "";

                if (treeViewItem == TreeRoot)
                {
                    sqlCommand = "DELETE FROM [User]";
                }
                else if (treeViewItem.Header is User)
                {
                    sqlCommand = $"DELETE FROM [User] WHERE [User].Id = {((User)treeViewItem.Header).id}";
                }
                else if (treeViewItem.Header is Character)
                {
                    sqlCommand = $"DELETE FROM [Character] WHERE [Character].Id = {((Character)treeViewItem.Header).id}";
                }
                else if (treeViewItem.Header is Item)
                {
                    sqlCommand = $"DELETE FROM [Item] WHERE [Item].Id = {((Item)treeViewItem.Header).id}";
                }
                else
                {
                    throw new Exception("???");
                }

                connection.Open();
                SqlCommand command = new SqlCommand(sqlCommand, connection);
                command.ExecuteNonQuery();

                if (!(treeViewItem == TreeRoot))
                    ((TreeViewItem)treeViewItem.Parent).Items.Remove(treeViewItem);
                else if (treeViewItem == TreeRoot)
                    TreeRoot.Items.Clear();
                else
                    throw new Exception("???");
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection.Close();
            }
            
        }

        private void UpdateSelected()
        {
            try
            {
                TreeViewItem targetViewItem = (TreeViewItem)DBtree.SelectedItem;
                string sqlCommand = "";
                object replacement = null;

                if (targetViewItem.Header is User)
                {
                    User user = (User)targetViewItem.Header;
                    user = GetUserFromInput(user);
                    if (user != null)
                    {
                        //MessageBox.Show(user.id + " " + user.name);
                        replacement = user;
                        sqlCommand = "UPDATE [User] " +
                            $"SET Name = '{user.name}' " +
                            $"WHERE [User].Id = {user.id}";
                    }
                }
                else if (targetViewItem.Header is Character)
                {
                    Character character = (Character)targetViewItem.Header;
                    character = GetCharacterFromInput(character);
                    if (character != null)
                    {
                        replacement = character;
                        sqlCommand = "UPDATE [Character] " +
                            $"SET " +
                            $"Name = '{character.name}', " +
                            $"Level = {character.level} " +
                            $"WHERE [Character].Id = {character.id}";
                    }
                }
                else if (targetViewItem.Header is Item)
                {
                    Item item = (Item)targetViewItem.Header;
                    item = GetItemFromInput(item);
                    if (item != null)
                    {
                        replacement = item;
                        sqlCommand = "UPDATE [Item] " +
                            $"SET " +
                            $"Name = '{item.name}', " +
                            $"Character_Id = {item.character.id} " +
                            $"WHERE [Item].Id = {item.id}";
                    }
                }
                else
                    throw new Exception("???");

                //MessageBox.Show(sqlCommand);
                if (replacement != null)
                {
                    connection.Open();
                    SqlCommand command = new SqlCommand(sqlCommand, connection);
                    command.ExecuteNonQuery();
                    targetViewItem.Header = replacement;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message);
            }
            finally
            {
                connection.Close();
            }
            
        }

        private bool DB_Insert(string tableName, Dictionary<string, object> columnValuePairs)
        {
            bool res = true;
            string sqlString;

            #region build SQL command
            string columns = "";
            string values = "";

            foreach (string column in columnValuePairs.Keys)
            {
                columns += column + ",";
                if (columnValuePairs[column] is string)
                    values += "'" + columnValuePairs[column].ToString() + "',";
                else
                    values += columnValuePairs[column].ToString() + ",";
            }

            sqlString = $"INSERT INTO {tableName} " +
                $"({columns.Substring(0, columns.Length - 1)}) VALUES " +
                $"({values.Substring(0, values.Length - 1)})";

            #endregion
            //MessageBox.Show(sqlString);
            try
            {
                connection.Open();

                SqlCommand sqlCommand = new SqlCommand(sqlString, connection);
                sqlCommand.ExecuteNonQuery();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                res = false;
            }
            finally
            {
                connection?.Close();
            }

            return res;
        }

        #region DB_Selects
        private void LoadUsersTo(TreeViewItem targetTreeItem)
        {
            try
            {
                connection.Open();
                string sqlString = "SELECT [User].ID, [User].Name FROM [User]";

                SqlCommand sqlCommand = new SqlCommand(sqlString, connection);
                SqlDataReader dr = sqlCommand.ExecuteReader();

                targetTreeItem.Items.Clear();
                while (dr.Read())
                {
                    User user = new User((int)dr[0], (string)dr[1]);

                    TreeViewItem treeItem = new TreeViewItem();
                    treeItem.Header = user;
                    targetTreeItem.Items.Add(treeItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
                foreach (var treeItem in targetTreeItem.Items)
                    LoadCharactersTo((TreeViewItem)treeItem);
            }
        }

        private void LoadCharactersTo(TreeViewItem targetTreeItem)
        {
            string sqlString = $"SELECT User_Id, Id, Name, Level FROM [Character] WHERE " +
                $"[Character].User_ID = {((User)targetTreeItem.Header).id}";

            try
            {
                connection.Open();

                SqlCommand sqlCommand = new SqlCommand(sqlString, connection);
                SqlDataReader dr = sqlCommand.ExecuteReader();

                targetTreeItem.Items.Clear();
                while (dr.Read())
                {
                    TreeViewItem treeItem = new TreeViewItem();
                    if(dr[3] is int)
                        treeItem.Header = new Character((int)dr[1], (User)targetTreeItem.Header, (string)dr[2], (int)dr[3]);
                    else
                        treeItem.Header = new Character((int)dr[1], (User)targetTreeItem.Header, (string)dr[2]);
                    targetTreeItem.Items.Add(treeItem);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
                foreach(var treeItem in targetTreeItem.Items)
                    LoadItemsTo((TreeViewItem)treeItem);
            }
        }

        private void LoadItemsTo(TreeViewItem targetTreeItem)
        {
            string sqlString = $"SELECT * FROM [Item] WHERE [Item].Character_ID = {((Character)targetTreeItem.Header).id}";

            try
            {
                connection.Open();

                SqlCommand sqlCommand = new SqlCommand(sqlString, connection);
                SqlDataReader dr = sqlCommand.ExecuteReader();

                targetTreeItem.Items.Clear();
                while (dr.Read())
                {
                    TreeViewItem item = new TreeViewItem();
                    item.Header = new Item((int)dr[1], ((Character)targetTreeItem.Header), (string)dr[2]);
                    targetTreeItem.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            finally
            {
                connection?.Close();
            }
        }
        #endregion
    }

    public class User
    {
        public int id;
        public string name;

        public override string ToString()
        {
            return name;
        }

        public User(int id, string name)
        {
            this.id = id;
            this.name = name;
        }
    }

    public class Character
    {
        public int id;
        public int level;
        public User user;
        public string name;

        public override string ToString()
        {
            return name + $"(level: {level})";
        }

        public Character(int id, User user, string name)
        {
            this.id = id;
            this.user = user;
            this.name = name;
        }

        public Character(int id, User user, string name, int level) : this(id, user, name)
        {
            this.level = level;
        }
    }

    public class Item
    {
        public Character character;
        public int id;
        public string name;

        public override string ToString()
        {
            return name;
        }

        public Item(int id, Character character, string name)
        {
            this.character = character;
            this.id = id;
            this.name = name;
        }
    }
}
