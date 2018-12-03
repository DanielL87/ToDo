using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using TaskList;

namespace TaskList.Models
{
    public class Item
    {
        private string _description;
        private int _id;
        private string _dueDate;

        public Item (string description)
        {
            _description = description;
        }

        public Item (string description, int id)
        {
            _description = description;
            _id = id;
        }

        public override bool Equals(System.Object otherItem)
        {
            if (!(otherItem is Item))
            {
                return false;
            }
            else
            {
                Item newItem = (Item) otherItem;
                bool areIdsEqual = (this.GetId() == newItem.GetId());
                bool areDescriptionsEqual = (this.GetDescription() == newItem.GetDescription());
                return (areIdsEqual && areDescriptionsEqual);
            }
        }

        public override int GetHashCode()
        {
            return this.GetDescription().GetHashCode();
        }


        public string GetDescription()
        {
            return _description;
        }

        // public void SetDescription(string newDescription)
        // {
        //     _description = newDescription;
        // }

        public int GetId()
        {
            return _id;
        }

        public string GetDueDate()
        {
            return _dueDate;
        }

     public static List<Item> GetAll()
        {
            List<Item> allItems = new List<Item> {};
            MySqlConnection conn = DB.Connection();
            conn.Open();
            MySqlCommand cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT * FROM items;";
            MySqlDataReader rdr = cmd.ExecuteReader() as MySqlDataReader;
            while(rdr.Read())
            {
            int itemId = rdr.GetInt32(0);
            string itemDescription = rdr.GetString(1);
            // Line below now only provides one argument!
            Item newItem = new Item(itemDescription);
            allItems.Add(newItem);
            }
            conn.Close();
            if (conn != null)
            {
            conn.Dispose();
            }
            return allItems;
        }

        public void Delete()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM tasks WHERE id = @ItemId; DELETE FROM categories_tasks WHERE task_id = @ItemId;";

            MySqlParameter itemIdParameter = new MySqlParameter();
            itemIdParameter.ParameterName = "@ItemId";
            itemIdParameter.Value = this.GetId();
            cmd.Parameters.Add(itemIdParameter);

            cmd.ExecuteNonQuery();
            if (conn != null)
            {
              conn.Close();
            }
        }

        public static void DeleteAll()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();

            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"DELETE FROM items;";

            cmd.ExecuteNonQuery();

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public void Save()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO items (description) VALUES (@ItemDescription);";
            MySqlParameter description = new MySqlParameter();
            description.ParameterName = "@ItemDescription";
            description.Value = this._description;
            cmd.Parameters.Add(description);
            cmd.ExecuteNonQuery();   
            _id = (int) cmd.LastInsertedId;

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public static Item Find(int id)
            {
                MySqlConnection conn = DB.Connection();
                conn.Open();

                var cmd = conn.CreateCommand() as MySqlCommand;
                cmd.CommandText = @"SELECT * FROM `items` WHERE id = @thisId;";

                MySqlParameter thisId = new MySqlParameter();
                thisId.ParameterName = "@thisId";
                thisId.Value = id;
                cmd.Parameters.Add(thisId);

                var rdr = cmd.ExecuteReader() as MySqlDataReader;

                int itemId = 0;
                string itemDescription = "";

                while (rdr.Read())
                {
                    itemId = rdr.GetInt32(0);
                    itemDescription = rdr.GetString(1);
                }

                Item foundItem= new Item(itemDescription, itemId);  // This line is new!

                    conn.Close();
                    if (conn != null)
                    {
                        conn.Dispose();
                    }

                return foundItem;  // This line is new!

            }

        public void Edit(string newDescription)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"UPDATE tasks SET description = @newDescription WHERE id = @searchId;";

            MySqlParameter searchId = new MySqlParameter();
            searchId.ParameterName = "@searchId";
            searchId.Value = _id;
            cmd.Parameters.Add(searchId);

            MySqlParameter description = new MySqlParameter();
            description.ParameterName = "@newDescription";
            description.Value = newDescription;
            cmd.Parameters.Add(description);

            cmd.ExecuteNonQuery();
            _description = newDescription;

            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public void AddCategory(Category newCategory)
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"INSERT INTO categories_tasks (category_id, task_id) VALUES (@CategoryId, @ItemId);";

            MySqlParameter category_id = new MySqlParameter();
            category_id.ParameterName = "@CategoryId";
            category_id.Value = newCategory.GetId();
            cmd.Parameters.Add(category_id);

            MySqlParameter task_id = new MySqlParameter();
            task_id.ParameterName = "@ItemId";
            task_id.Value = _id;
            cmd.Parameters.Add(task_id);

            cmd.ExecuteNonQuery();
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
        }

        public List<Category> GetCategories()
        {
            MySqlConnection conn = DB.Connection();
            conn.Open();
            var cmd = conn.CreateCommand() as MySqlCommand;
            cmd.CommandText = @"SELECT category_id FROM categories_tasks WHERE task_id = @itemId;";

            cmd.Parameters.Add(new MySqlParameter("@itemId", _id));

            var rdr = cmd.ExecuteReader() as MySqlDataReader;

            List<int> categoryIds = new List<int> {};
            while(rdr.Read())
            {
                int categoryId = rdr.GetInt32(0);
                categoryIds.Add(categoryId);
            }
            rdr.Dispose();

            List<Category> categories = new List<Category> {};
            foreach (int categoryId in categoryIds)
            {
                var categoryQuery = conn.CreateCommand() as MySqlCommand;
                categoryQuery.CommandText = @"SELECT * FROM categories WHERE id = @CategoryId;";

                categoryQuery.Parameters.Add(new MySqlParameter("@CategoryId", categoryId));

                var categoryQueryRdr = categoryQuery.ExecuteReader() as MySqlDataReader;
                while(categoryQueryRdr.Read())
                {
                    int thisCategoryId = categoryQueryRdr.GetInt32(0);
                    string categoryName = categoryQueryRdr.GetString(1);
                    Category foundCategory = new Category(categoryName, thisCategoryId);
                    categories.Add(foundCategory);
                }
                categoryQueryRdr.Dispose();
            }
            conn.Close();
            if (conn != null)
            {
                conn.Dispose();
            }
            return categories;
        }
    }
}
