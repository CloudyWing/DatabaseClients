using System;
using System.Data;

namespace CloudyWing.DatabaseClients {
    public sealed class ParameterMetadata : ICloneable {
        private string parameterName;

        public ParameterMetadata() { }

        internal ParameterMetadata(ParameterMetadata from) {
            ParameterName = from.ParameterName;
            Direction = from.Direction;
            DbType = from.DbType;
            Size = from.Size;
            Precision = from.Precision;
            Scale = from.Scale;
            SourceColumn = from.SourceColumn;
            SourceVersion = from.SourceVersion;
            Value = from.Value;
        }

        internal ParameterMetadata(IDbDataParameter from) {
            ParameterName = from.ParameterName;
            Direction = from.Direction;
            DbType = from.DbType;
            Size = from.Size;
            Precision = from.Precision;
            Scale = from.Scale;
            SourceColumn = from.SourceColumn;
            SourceVersion = from.SourceVersion;
            Value = from.Value;
        }

        public string ParameterName {
            get {
                return parameterName;
            }
            set {
                string name = value?.Trim();
                if (!string.IsNullOrEmpty(name)) {
                    switch (name[0]) {
                        case '@':
                        case ':':
                        case '?':
                            parameterName = name.Substring(1);
                            return;
                    }
                }
                parameterName = name;
            }
        }

        public object Value { get; set; }
        public ParameterDirection Direction { get; set; } = ParameterDirection.Input;
        public DbType? DbType { get; set; }
        public int? Size { get; set; }
        public byte? Precision { get; set; }
        public byte? Scale { get; set; }
        public string SourceColumn { get; set; }
        public DataRowVersion? SourceVersion { get; set; }

        public object Clone() => new ParameterMetadata(this);

        public void ApplyParameter(IDbDataParameter to) {
            to.ParameterName = ParameterName;
            to.Direction = Direction;

            // 雖然DbDataParameter的Property都有預設值
            // 但避免因set造成一些額外變動
            // 如DbType是預設String
            // 但只有完全沒set過DbType的情況下，DbType才會依Value type自動調整
            // 所以Metadata只將有值的項目設定到DbDataParameter
            if (DbType.HasValue) {
                to.DbType = DbType.Value;
            }
            if (Size.HasValue) {
                to.Size = Size.Value;
            }
            if (Precision.HasValue) {
                to.Precision = Precision.Value;
            }
            if (Precision.HasValue) {
                to.Scale = Scale.Value;
            }
            if (SourceColumn != null) {
                to.SourceColumn = SourceColumn;
            }
            if (SourceVersion.HasValue) {
                to.SourceVersion = SourceVersion.Value;
            }
            to.Value = Value;
        }
    }
}