using System.Data;
using System.Reflection;

namespace signalR.Utils
{
    public static class Utils
    {
        public static DataTable ToDataTable<T>(this List<T> items)
        {
            var dataTable = new DataTable(typeof(T).Name);

            // Obtener todas las propiedades del tipo T.
            PropertyInfo[] props = typeof(T).GetProperties(BindingFlags.Public | BindingFlags.Instance);

            // Crear una columna en el DataTable para cada propiedad pública de T.
            foreach (var prop in props)
            {
                dataTable.Columns.Add(prop.Name, prop.PropertyType);
            }

            // Rellenar el DataTable con los valores de las propiedades de cada item en la lista.
            foreach (T item in items)
            {
                var values = new object[props.Length];
                for (int i = 0; i < props.Length; i++)
                {
                    // Insertar el valor de la propiedad i-ésima.
                    values[i] = props[i].GetValue(item, null);
                }

                dataTable.Rows.Add(values);
            }

            return dataTable;
        }

        public static DataTable StringsToDataTable(List<string> items)
        {
            var dataTable = new DataTable();

            // Agregar una sola columna al DataTable
            dataTable.Columns.Add("Value", typeof(string));

            // Rellenar el DataTable con los strings de la lista
            foreach (var item in items)
            {
                dataTable.Rows.Add(item);
            }

            return dataTable;
        }


        public static DataTable LongsToDataTable(List<long> items)
        {
            var dataTable = new DataTable();

            // Agregar una sola columna al DataTable
            dataTable.Columns.Add("Value", typeof(long));

            // Rellenar el DataTable con los strings de la lista
            foreach (var item in items)
            {
                dataTable.Rows.Add(item);
            }

            return dataTable;
        }
    }
}
