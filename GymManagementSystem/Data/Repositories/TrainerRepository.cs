using System;
using System.Collections.Generic;
using Microsoft.Data.Sqlite;
using GymManagementSystem.Data;
using GymManagementSystem.Models;

namespace GymManagementSystem.Data.Repositories
{
    public class TrainerRepository
    {
        public List<Trainer> GetAll()
        {
            var list = new List<Trainer>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT trainer_id, name, specialization, contact, availability FROM trainers ORDER BY trainer_id ASC;";

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Trainer
                {
                    TrainerId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Specialization = reader.GetString(2),
                    Contact = reader.GetString(3),
                    Availability = reader.GetString(4)
                });
            }
            return list;
        }

        public Trainer? GetById(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT trainer_id, name, specialization, contact, availability FROM trainers WHERE trainer_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Trainer
                {
                    TrainerId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Specialization = reader.GetString(2),
                    Contact = reader.GetString(3),
                    Availability = reader.GetString(4)
                };
            }
            return null;
        }

        public List<Trainer> Search(string query)
        {
            var list = new List<Trainer>();
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                SELECT trainer_id, name, specialization, contact, availability 
                FROM trainers 
                WHERE name LIKE @q OR specialization LIKE @q OR contact LIKE @q
                ORDER BY name ASC;
            ";
            cmd.Parameters.AddWithValue("@q", $"%{query}%");

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                list.Add(new Trainer
                {
                    TrainerId = reader.GetInt32(0),
                    FullName = reader.GetString(1),
                    Specialization = reader.GetString(2),
                    Contact = reader.GetString(3),
                    Availability = reader.GetString(4)
                });
            }
            return list;
        }

        public int Add(Trainer trainer)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                INSERT INTO trainers (name, specialization, contact, availability)
                VALUES (@name, @spec, @contact, @avail);
                SELECT last_insert_rowid();
            ";
            cmd.Parameters.AddWithValue("@name", trainer.FullName);
            cmd.Parameters.AddWithValue("@spec", trainer.Specialization);
            cmd.Parameters.AddWithValue("@contact", trainer.Contact);
            cmd.Parameters.AddWithValue("@avail", trainer.Availability);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public bool Update(Trainer trainer)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                UPDATE trainers 
                SET name = @name, specialization = @spec, contact = @contact, availability = @avail
                WHERE trainer_id = @id;
            ";
            cmd.Parameters.AddWithValue("@id", trainer.TrainerId);
            cmd.Parameters.AddWithValue("@name", trainer.FullName);
            cmd.Parameters.AddWithValue("@spec", trainer.Specialization);
            cmd.Parameters.AddWithValue("@contact", trainer.Contact);
            cmd.Parameters.AddWithValue("@avail", trainer.Availability);

            return cmd.ExecuteNonQuery() > 0;
        }

        public bool Delete(int id)
        {
            using var conn = DatabaseHelper.GetConnection();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "DELETE FROM trainers WHERE trainer_id = @id;";
            cmd.Parameters.AddWithValue("@id", id);
            return cmd.ExecuteNonQuery() > 0;
        }
    }
}
