using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;

namespace CloudyWing.DatabaseClients {
    public sealed class ParameterCollection : KeyedCollection<string, ParameterMetadata> {
        protected override string GetKeyForItem(ParameterMetadata item) {
            return item.ParameterName;
        }

        public void Add(string parameterName, object value) {
            Add(new ParameterMetadata {
                ParameterName = parameterName,
                Value = value,
            });
        }

        public void Add(string parameterName, object value, DbType dbType) {
            Add(new ParameterMetadata {
                ParameterName = parameterName,
                Value = value,
                DbType = dbType
            });
        }

        public void Add(string parameterName, object value, DbType dbType, int size) {
            Add(new ParameterMetadata {
                ParameterName = parameterName,
                Value = value,
                DbType = dbType,
                Size = size
            });
        }

        public void Add(string parameterName, object value, DbType dbType, byte precision, byte scale) {
            Add(new ParameterMetadata {
                ParameterName = parameterName,
                Value = value,
                DbType = dbType,
                Precision = precision,
                Scale = scale,
            });
        }

        public void Add(string parameterName, object value, DbType dbType, ParameterDirection direction) {
            Add(new ParameterMetadata {
                ParameterName = parameterName,
                Value = value,
                Direction = direction,
                DbType = dbType
            });
        }

        public void Add(IDbDataParameter parameter) {
            base.Add(new ParameterMetadata {
                ParameterName = parameter.ParameterName,
                Value = parameter.Value,
                DbType = parameter.DbType,
                Size = parameter.Size,
                Precision = parameter.Precision,
                Scale = parameter.Scale,
                Direction = parameter.Direction,
                SourceColumn = parameter.SourceColumn,
                SourceVersion = parameter.SourceVersion
            });
        }

        public void AddRange(params ParameterMetadata[] parameters) {
            AddRange(parameters as IEnumerable<ParameterMetadata>);
        }

        public void AddRange(IEnumerable<ParameterMetadata> parameters) {
            if (parameters == null) {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (ParameterMetadata parameter in parameters) {
                Add(parameter);
            }
        }

        public void AddRange(params IDbDataParameter[] parameters) {
            AddRange(parameters as IEnumerable<IDbDataParameter>);
        }

        public void AddRange(IEnumerable<IDbDataParameter> parameters) {
            if (parameters == null) {
                throw new ArgumentNullException(nameof(parameters));
            }

            foreach (IDbDataParameter parameter in parameters) {
                Add(new ParameterMetadata(parameter));
            }
        }

        public void AddRange(IDictionary<string, object> pairs) {
            if (pairs == null) {
                throw new ArgumentNullException(nameof(pairs));
            }

            foreach (KeyValuePair<string, object> pair in pairs) {
                Add(pair.Key, pair.Value);
            }
        }

        public void AddRange(object obj) {
            Debug.Assert(
                !(obj is string
                    || obj is IEnumerable<ParameterMetadata>
                    || obj is IEnumerable<IDbDataParameter>
                    || obj is IDictionary<string, object>
                ), "obj: obj type is error."
            );

            PropertyDescriptorCollection props = TypeDescriptor.GetProperties(obj);
            foreach (PropertyDescriptor prop in props) {
                object val = prop.GetValue(obj);
                Add(prop.Name, val);
            }
        }
    }
}