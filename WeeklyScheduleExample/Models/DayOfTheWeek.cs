﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml.Schema;
using System.Xml.Serialization;
using WeeklyScheduleExample.App_GlobalResources;

namespace WeeklyScheduleExample.Models
{
	public class DayOfTheWeek : IXmlSerializable
	{
		#region Private Variables

		private WorkingTime workingTime;

		private WorkHours workHours;

		private List<BreakHours> breaks = new List<BreakHours>();

		#endregion

		#region Constructor

		public DayOfTheWeek()
		{
			this.DayOfWeek = DayOfWeek.Sunday;
		}

		public DayOfTheWeek(DayOfWeek dayOfWeek)
		{
			this.DayOfWeek = dayOfWeek;
		}

		#endregion Constructor

		#region Public Properties

		public DayOfWeek DayOfWeek { get; internal set; }

		public WorkingTime WorkingTime
		{
			get { return this.workingTime; }
			set
			{
				if (this.workingTime != value)
				{
					this.workingTime = value;

					switch (value)
					{
						case WorkingTime.Closed:
							this.workHours = null;
							this.breaks = null;
							break;

						case WorkingTime.RoundTheClock:
							this.workHours = null;
							break;
					}
				}
			}
		}

		public WorkHours WorkHours
		{
			get { return this.workHours; }
			set
			{
				if (value == null)
					throw new ArgumentNullException("value");

				this.workHours = value;
				this.workingTime = WorkingTime.WorkHours;
			}
		}

		public IList<BreakHours> Breaks
		{
			get { return this.breaks; }
		}

		#endregion

		#region Public Methods

		public void AddNewBreak(BreakHours breakHours)
		{
			if (breakHours == null)
				throw new ArgumentNullException("breakHours");

			if (this.breaks == null)
				this.breaks = new List<BreakHours>();

			if (this.WorkingTime != WorkingTime.WorkHours && this.WorkingTime != WorkingTime.RoundTheClock)
                throw new InvalidOperationException(string.Format(WeekModel.cultureInfo, Resources.E_BreakCanNotBeAddedForTypeWithParam, this.WorkingTime.ToString()));

			this.breaks.Add(breakHours);
		}

		#endregion

		#region IXmlSerializable Members

		/// <summary>
		/// This method is reserved and should not be used
		/// </summary>
		/// <returns>An System.Xml.Schema.XmlSchema that describes the XML representation of the object
		/// that is produced by the System.Xml.Serialization.IXmlSerializable.WriteXml(System.Xml.XmlWriter) method 
		/// and consumed by the System.Xml.Serialization.IXmlSerializable.ReadXml(System.Xml.XmlReader) method
		/// </returns>
		public System.Xml.Schema.XmlSchema GetSchema()
		{
			Assembly currentAssembly = Assembly.GetAssembly(typeof(DayOfTheWeek));
			using (Stream xsdStream = currentAssembly.GetManifestResourceStream(currentAssembly.GetName() + ".WeeklySchedule.xsd"))
				return XmlSchema.Read(xsdStream, null);
		}

		/// <summary>
		/// Generates an object from its XML representation
		/// </summary>
		/// <param name="reader">The System.Xml.XmlReader stream from which the object is deserialized</param>
		public void ReadXml(System.Xml.XmlReader reader)
		{
			if (reader.IsEmptyElement)
                throw new InvalidOperationException(string.Format(WeekModel.cultureInfo, Resources.E_ElementCanNotBeEmptyWithParam, reader.Name));

			string startNodeName = reader.Name;
			while (reader.Read())
			{
				if (!reader.IsStartElement())
				{
					if (startNodeName == reader.Name)
					{
						reader.Read();//Set pointer to the next node
						return;
					}
					else
					{
						continue;
					}
				}

				switch (reader.Name)
				{
					case "closed":
						if (this.WorkingTime != WorkingTime.None)
                            throw new InvalidOperationException(string.Format(WeekModel.cultureInfo, Resources.E_WorkingTimeMustBeNoneWithParams2, this.WorkingTime.ToString(), startNodeName));

						this.WorkingTime = WorkingTime.Closed;
						break;

					case "round_the_clock":
						if (this.WorkingTime != WorkingTime.None)
                            throw new InvalidOperationException(string.Format(WeekModel.cultureInfo, Resources.E_WorkingTimeMustBeNoneWithParams2, this.WorkingTime.ToString(), startNodeName));

						this.WorkingTime = WorkingTime.RoundTheClock;
						break;

					case "work_hours":
						if (this.WorkingTime != WorkingTime.None)
                            throw new InvalidOperationException(string.Format(WeekModel.cultureInfo, Resources.E_WorkingTimeMustBeNoneWithParams2, this.WorkingTime.ToString(), startNodeName));

						this.WorkHours = new WorkHours();
						this.WorkHours.ReadXml(reader);
						break;

					case "break":
						BreakHours breakHours = new BreakHours();
						breakHours.ReadXml(reader);
						this.AddNewBreak(breakHours);
						break;

					default:
                        throw new InvalidOperationException(string.Format(WeekModel.cultureInfo, Resources.E_UnexpectedXmlElementWithParam, reader.Name));
				}
			}
		}

		/// <summary>
		/// Converts an object into its XML representation
		/// </summary>
		/// <param name="writer">The System.Xml.XmlWriter stream to which the object is serialized</param>
		public void WriteXml(System.Xml.XmlWriter writer)
		{
			switch (this.WorkingTime)
			{
				case WorkingTime.Closed:
					writer.WriteElementString("closed", null);
					return;

				case WorkingTime.WorkHours:
					writer.WriteStartElement("work_hours");
					this.WorkHours.WriteXml(writer);
					writer.WriteEndElement();
					break;

				case WorkingTime.RoundTheClock:
					writer.WriteElementString("round_the_clock", null);
					break;

				case WorkingTime.None:
					throw new InvalidOperationException(Resources.E_UndefinedWorkingTime);
			}

			if (this.Breaks == null)
				return;

			foreach (BreakHours breakHours in this.Breaks)
			{
				writer.WriteStartElement("break");
				breakHours.WriteXml(writer);
				writer.WriteEndElement();
			}
		}

		#endregion

		#region Equality Operations

		public override bool Equals(object obj)
		{
			return Equals(obj as DayOfTheWeek);
		}

		public bool Equals(DayOfTheWeek obj)
		{
			if (object.ReferenceEquals(obj, null))
			{
				return false;
			}
			return DayOfTheWeek.Equals(this, obj);
		}

		public static bool Equals(DayOfTheWeek obj1, DayOfTheWeek obj2)
		{
			if (object.ReferenceEquals(obj1, null) && object.ReferenceEquals(obj2, null))
			{
				return true;
			}

			if (object.ReferenceEquals(obj1, null) || object.ReferenceEquals(obj2, null))
			{
				return false;
			}

			bool breaksAreEqual;
			if (obj1.breaks != null && obj2.breaks != null)
				breaksAreEqual = obj1.breaks.SequenceEqual(obj2.breaks);
			else
				breaksAreEqual = obj1.breaks == null && obj2.breaks == null;

			return obj1.workingTime == obj2.workingTime
				&& obj1.workHours == obj2.workHours
				&& obj1.DayOfWeek == obj2.DayOfWeek
				&& breaksAreEqual;
		}

		public static bool operator ==(DayOfTheWeek obj1, DayOfTheWeek obj2)
		{
			return DayOfTheWeek.Equals(obj1, obj2);
		}

		public static bool operator !=(DayOfTheWeek obj1, DayOfTheWeek obj2)
		{
			return !DayOfTheWeek.Equals(obj1, obj2);
		}

		public override int GetHashCode()
		{
			int result = this.workingTime.GetHashCode()
				^ this.DayOfWeek.GetHashCode();

			if (this.workHours != null)
				result ^= this.workHours.GetHashCode();

			if (this.breaks != null)
				for (int i = 0; i < this.breaks.Count; i++)
					result ^= this.breaks[i].GetHashCode();

			return result;
		}

		#endregion
	}
}