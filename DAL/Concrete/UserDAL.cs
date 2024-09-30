using DAL.Interface;
using DTO;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;

namespace DAL.Concrete
{
    public class UserDal : IUserDAL
    {
        private readonly SqlConnection _connection;

        public UserDal(string connectionString)
        {
            _connection = new SqlConnection(connectionString);
        }

        public User Create(User user)
        {
            try
            {
                _connection.Open();

                // Отримання наступного доступного user_id
                int nextId;
                using (var cmd = new SqlCommand("SELECT ISNULL(MAX(user_id), 0) + 1 FROM users", _connection))
                {
                    nextId = Convert.ToInt32(cmd.ExecuteScalar());
                }

                using (SqlCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "INSERT INTO users (user_id, user_name, user_password, role) " +
                                          "VALUES (@UserId, @UserName, @UserPassword, @Role);";

                    // Додаємо параметри
                    command.Parameters.Add("@UserId", SqlDbType.Int).Value = nextId;
                    command.Parameters.Add("@UserName", SqlDbType.NVarChar).Value = user.UserName;
                    command.Parameters.Add("@UserPassword", SqlDbType.NVarChar).Value = user.UserPassword;
                    command.Parameters.Add("@Role", SqlDbType.NVarChar).Value = user.Role;

                    command.ExecuteNonQuery();
                }

                user.UserId = nextId; // Присвоюємо user_id новому користувачу
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error while inserting user: {ex.Message}");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }

            return user;
        }


        public User Delete(int id)
        {
            User deletedUser = GetById(id);
            if (deletedUser == null)
                return null;

            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "DELETE FROM users WHERE user_id = @UserId"; // Використовуємо правильне ім'я стовпця
                command.Parameters.AddWithValue("@UserId", id);

                _connection.Open();
                command.ExecuteNonQuery();
                _connection.Close();
            }

            return deletedUser; // Повертаємо видаленого користувача
        }




        public List<User> GetAll()
        {
            var users = new List<User>();

            try
            {
                _connection.Open();

                using (SqlCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT user_id, user_name, user_password, role FROM users"; // Використовуємо правильну назву стовпця

                    using (SqlDataReader reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            users.Add(new User
                            {
                                UserId = Convert.ToInt32(reader["user_id"]), // Використовуємо правильну назву стовпця
                                UserName = reader["user_name"].ToString(),
                                UserPassword = reader["user_password"].ToString(),
                                Role = reader["role"].ToString()
                            });
                        }
                    }
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error while retrieving users: {ex.Message}");
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
            }

            return users;
        }

        public User GetByCredentials(string userName, string userPassword)
        {
            try
            {
                _connection.Open(); 

                using (SqlCommand command = _connection.CreateCommand())
                {
                    command.CommandText = "SELECT user_id, user_name, user_password, role FROM users WHERE user_name = @UserName AND user_password = @UserPassword";
                    command.Parameters.AddWithValue("@UserName", userName);
                    command.Parameters.AddWithValue("@UserPassword", userPassword);

                    SqlDataReader reader = command.ExecuteReader();

                    if (reader.Read())
                    {
                        var user = new User
                        {
                            UserId = Convert.ToInt32(reader["user_id"]),
                            UserName = reader["user_name"].ToString(),
                            UserPassword = reader["user_password"].ToString(),
                            Role = reader["role"].ToString()
                        };

                        return user;
                    }

                    return null;
                }
            }
            catch (SqlException ex)
            {
                Console.WriteLine($"Error while retrieving user: {ex.Message}");
                return null;
            }
            finally
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close(); 
                }
            }
        }


        public User GetById(int id)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "SELECT user_id, user_name, user_password, role FROM users WHERE user_id = @Id"; // Змінено 'id' на 'user_id'
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                SqlDataReader reader = command.ExecuteReader();

                if (reader.Read())
                {
                    var user = new User
                    {
                        UserId = Convert.ToInt32(reader["user_id"]),
                        UserName = reader["user_name"].ToString(),
                        UserPassword = reader["user_password"].ToString(),
                        Role = reader["role"].ToString()
                    };

                    _connection.Close();
                    return user;
                }

                _connection.Close();
                return null;
            }
        }


        public User Update(int id, User user)
        {
            using (SqlCommand command = _connection.CreateCommand())
            {
                command.CommandText = "UPDATE users SET user_name = @UserName, user_password = @UserPassword, " +
                                      "role = @Role WHERE id = @Id";

                command.Parameters.AddWithValue("@UserName", user.UserName);
                command.Parameters.AddWithValue("@UserPassword", user.UserPassword);
                command.Parameters.AddWithValue("@Role", user.Role);
                command.Parameters.AddWithValue("@Id", id);

                _connection.Open();
                command.ExecuteNonQuery();
                _connection.Close();

                return GetById(id);
            }
        }
    }
}
