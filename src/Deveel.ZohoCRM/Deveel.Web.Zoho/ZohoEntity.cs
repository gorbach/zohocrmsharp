﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Xml.Linq;

namespace Deveel.Web.Zoho {
	[Serializable]
	public abstract class ZohoEntity : ISerializable {
		private readonly Dictionary<string, object> fields = new Dictionary<string, object>();

		protected ZohoEntity() {
			var entityName = Attribute.GetCustomAttribute(GetType(), typeof (EntityNameAttribute)) as EntityNameAttribute;
			if (entityName == null)
				throw new InvalidOperationException();

			EntityName = entityName.EntityName;
		}

		protected ZohoEntity(SerializationInfo info, StreamingContext context) {
			foreach (var entry in info) {
				fields[entry.Name] = entry.Value;
			}
		}

		internal string EntityName { get; private set; }

		public virtual string Id {
			get { return null; }
		}

		public bool HasValue(string fieldName) {
			return fields.ContainsKey(fieldName);
		}

		public T GetValue<T>(string fieldName) {
			object value;
			if (!fields.TryGetValue(fieldName, out value))
				return default(T);

			if (!(value is T))
				value = Convert.ChangeType(value, typeof (T));

			return (T) value;
		}

		public void SetValue<T>(string fieldName, T value) {
			if (Equals(default(T), value)) {
				fields.Remove(fieldName);
			} else if (!typeof (T).IsPrimitive) {
				fields[fieldName] = value.ToString();
			} else {
				fields[fieldName] = value;
			}
		}

		public void SetValue(string fieldName, string value) {
			SetValue<string>(fieldName, value);
		}

		public void SetValue(string fieldName, byte value) {
			SetValue<byte>(fieldName, value);
		}

		[CLSCompliant(false)]
		public void SetValue(string fieldName, sbyte value) {
			SetValue<sbyte>(fieldName, value);
		}

		public void SetValue(string fieldName, short value) {
			SetValue<short>(fieldName, value);
		}

		[CLSCompliant(false)]
		public void SetValue(string fieldName, ushort value) {
			SetValue<ushort>(fieldName, value);
		}

		public void SetValue(string fieldName, int value) {
			SetValue<int>(fieldName, value);
		}

		[CLSCompliant(false)]
		public void SetValue(string fieldName, uint value) {
			SetValue<uint>(fieldName, value);
		}

		public void SetValue(string fieldName, long value) {
			SetValue<long>(fieldName, value);
		}

		[CLSCompliant(false)]
		public void SetValue(string fieldName, ulong value) {
			SetValue<ulong>(fieldName, value);
		}

		public void SetValue(string fieldName, float value) {
			SetValue<float>(fieldName, value);
		}

		public void SetValue(string fieldName, double value) {
			SetValue<double>(fieldName, value);
		}

		public void SetValue(string fieldName, decimal value) {
			SetValue<decimal>(fieldName, value);
		}

		public void SetValue(string fieldName, char value) {
			SetValue<char>(fieldName, value);
		}

		public void SetValue(string fieldName, DateTime value) {
			SetValue<DateTime>(fieldName, value);
		}

		public string GetString(string fieldName) {
			return GetValue<string>(fieldName);
		}

		public byte GetByte(string fieldName) {
			return GetValue<byte>(fieldName);
		}

		[CLSCompliant(false)]
		public sbyte GetSByte(string fieldName) {
			return GetValue<sbyte>(fieldName);
		}

		public short GetInt16(string fieldName) {
			return GetValue<short>(fieldName);
		}

		[CLSCompliant(false)]
		public ushort GetUInt16(string fieldName) {
			return GetValue<ushort>(fieldName);
		}

		public int GetInt32(string fieldName) {
			return GetValue<int>(fieldName);
		}

		[CLSCompliant(false)]
		public uint GetUInt32(string fieldName) {
			return GetValue<uint>(fieldName);
		}

		public long GetInt64(string fieldName) {
			return GetValue<long>(fieldName);
		}

		[CLSCompliant(false)]
		public ulong GetUInt64(string fieldName) {
			return GetValue<ulong>(fieldName);
		}

		public float GetSingle(string fieldName) {
			return GetValue<float>(fieldName);
		}

		public double GetDouble(string fieldName) {
			return GetValue<double>(fieldName);
		}

		public decimal GetDecimal(string fieldName) {
			return GetValue<decimal>(fieldName);
		}

		public DateTime GetDateTime(string fieldName) {
			return GetValue<DateTime>(fieldName);
		}

		public char GetChar(string fieldName) {
			return GetValue<char>(fieldName);
		}

		// boolean is a spacial case
		public bool GetBoolean(string fieldName) {
			object value;
			if (!fields.TryGetValue(fieldName, out value))
				return false;

			if (value is bool)
				return (bool) value;
			if (value is string) {
				if ((string)value == "0")
					return false;
				if ((string)value == "1")
					return true;
				if ((string)value == "true")
					return true;
				if ((string)value == "false")
					return false;

				throw new FormatException("String value was not a valid boolean");
			}

			//TODO: handle numerics ...

			return false;
		}

		internal virtual void AppendTo(XElement parent, int rowNum = 1) {
			var element = new XElement("row");
			element.SetAttributeValue("no", rowNum);
			AppendFieldsToRow(element);
			parent.Add(element);
		}

		internal virtual  void AppendFieldsToRow(XElement rowElement) {
			foreach (var field in fields) {
				var fieldElement = new XElement("FL");
				fieldElement.SetAttributeValue("val", field.Key);
				fieldElement.Add(field.Value);
				rowElement.Add(fieldElement);
			}			
		}

		public XElement ToXml() {
			var root = new XElement(EntityName);
			AppendTo(root);
			return root;
		}

		internal virtual void LoadFromXml(XElement element) {
			var fieldNodes = element.Descendants();
			foreach (var node in fieldNodes) {
				LoadFieldFromXml(node);
			}
		}

		internal virtual void LoadFieldFromXml(XElement fieldElement) {
			var key = fieldElement.Attribute("val");
			var value = fieldElement.Nodes().OfType<XText>().FirstOrDefault();

			if (key == null)
				throw new FormatException();
			if (value == null)
				throw new FormatException();

			SetValue(key.Value, value.Value);
		}

		public virtual void GetObjectData(SerializationInfo info, StreamingContext context) {
			foreach (var field in fields) {
				info.AddValue(field.Key, field.Value);
			}
		}
	}
}