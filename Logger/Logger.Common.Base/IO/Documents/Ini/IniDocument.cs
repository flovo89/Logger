using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Runtime.Serialization;
using System.Text;

using Claymount.Console.Common.IO.Documents.Ini;

using Logger.Common.Collections.Generic;
using Logger.Common.DataTypes;
using Logger.Common.ObjectModel;




namespace Logger.Common.IO.Documents.Ini
{
    [Serializable]
    public class IniDocument : ICloneable<IniDocument>,
            ISerializable
    {
        #region Constants

        private const string DocumentSerializationName = "Document";

        #endregion




        #region Instance Constructor/Destructor

        public IniDocument ()
        {
            this.Sections = new IniSectionCollection();
        }

        protected IniDocument (SerializationInfo info, StreamingContext context)
                : this()
        {
            string content = info.GetString(IniDocument.DocumentSerializationName);
            this.Load(content);
        }

        #endregion




        #region Instance Properties/Indexer

        public string DebugValue
        {
            get
            {
                return this.ToString();
            }
        }

        public IniSectionCollection Sections { get; }

        #endregion




        #region Instance Methods

        public void InjectDictionary (IDictionary<string, IDictionary<string, string>> dictionary, bool overwriteExisting)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            IDictionary<string, IDictionary<string, IList<string>>> dictionarySections = new Dictionary<string, IDictionary<string, IList<string>>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string, IDictionary<string, string>> section in dictionary)
            {
                IDictionary<string, IList<string>> dictionarySection = new Dictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);

                foreach (KeyValuePair<string, string> element in section.Value)
                {
                    dictionarySection.Add(element.Key, new List<string>());
                    dictionarySection[element.Key].Add(element.Value);
                }

                dictionarySections.Add(section.Key, dictionarySection);
            }

            this.InjectDictionary(dictionarySections, overwriteExisting);
        }

        public void InjectDictionary (IDictionary<string, IDictionary<string, IList<string>>> dictionary, bool overwriteExisting)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            foreach (KeyValuePair<string, IDictionary<string, IList<string>>> dictionarySection in dictionary)
            {
                if (!this.Sections.ContainsSection(dictionarySection.Key))
                {
                    this.Sections.Add(new IniSection(dictionarySection.Key));
                }

                IniSection section = this.Sections[dictionarySection.Key];

                foreach (KeyValuePair<string, IList<string>> dictionaryElements in dictionarySection.Value)
                {
                    if (overwriteExisting)
                    {
                        this.RemoveElements((s, e) =>
                        {
                            if (string.Equals(s.Name, dictionarySection.Key, StringComparison.InvariantCultureIgnoreCase))
                            {
                                if (e is IniValueElement)
                                {
                                    IniValueElement e2 = (IniValueElement)e;

                                    return string.Equals(e2.Key, dictionaryElements.Key, StringComparison.InvariantCultureIgnoreCase);
                                }
                            }

                            return false;
                        });
                    }

                    int index = -1;

                    List<IniSectionElement> elementsToRemove = new List<IniSectionElement>();

                    foreach (IniSectionElement element in section.Elements)
                    {
                        if (!( element is IniValueElement ))
                        {
                            continue;
                        }

                        IniValueElement valueElement = (IniValueElement)element;

                        if (string.Equals(valueElement.Key, dictionaryElements.Key, StringComparison.InvariantCultureIgnoreCase))
                        {
                            index++;
                        }
                        else
                        {
                            continue;
                        }

                        if (index < dictionaryElements.Value.Count)
                        {
                            valueElement.Value = dictionaryElements.Value[index];
                        }
                        else
                        {
                            elementsToRemove.Add(valueElement);
                        }
                    }

                    index++;

                    section.Elements.RemoveRangeAll(elementsToRemove);

                    for (int i4 = index; i4 < dictionaryElements.Value.Count; i4++)
                    {
                        section.Elements.Add(new IniValueElement(dictionaryElements.Key, dictionaryElements.Value[i4]));
                    }
                }
            }
        }

        public void Load (IniReader reader)
        {
            if (reader == null)
            {
                throw new ArgumentNullException(nameof(reader));
            }

            this.Sections.Clear();

            IniSection currentSection = null;

            while (reader.Read())
            {
                if (reader.CurrentSection != null)
                {
                    currentSection = new IniSection(reader.CurrentSection.Name);
                    this.Sections.Add(currentSection);
                }

                if (reader.CurrentElement != null)
                {
                    if (currentSection == null)
                    {
                        currentSection = new IniSection(string.Empty);
                        this.Sections.Add(currentSection);
                    }

                    currentSection.Elements.Add(reader.CurrentElement);
                }
            }
        }

        public void Load (TextReader reader)
        {
            this.Load(new IniReader(reader, true));
        }

        public void Load (string str)
        {
            if (str == null)
            {
                throw new ArgumentNullException(nameof(str));
            }

            using (StringReader r = new StringReader(str))
            {
                this.Load(r);
            }
        }

        public void Load (Stream stream)
        {
            this.Load(new IniReader(stream, true));
        }

        public void Load (Stream stream, Encoding encoding)
        {
            this.Load(new IniReader(stream, true, encoding));
        }

        public void Load (Stream stream, Encoding encoding, bool detectByteOrderFromTextEncodingMarks)
        {
            this.Load(new IniReader(stream, true, encoding, detectByteOrderFromTextEncodingMarks));
        }

        public void Load (Stream stream, Encoding encoding, bool detectByteOrderFromTextEncodingMarks, int bufferSize)
        {
            this.Load(new IniReader(stream, true, encoding, detectByteOrderFromTextEncodingMarks, bufferSize));
        }

        public void Load (IDictionary<string, IDictionary<string, string>> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            this.Sections.Clear();

            this.InjectDictionary(dictionary, true);
        }

        public void Load (IDictionary<string, IDictionary<string, IList<string>>> dictionary)
        {
            if (dictionary == null)
            {
                throw new ArgumentNullException(nameof(dictionary));
            }

            this.Sections.Clear();

            this.InjectDictionary(dictionary, true);
        }

        public void MergeSections ()
        {
            List<IniSection> mergedSectionList = new List<IniSection>();

            KeyedCollectionWrapper<string, IniSection> mergedSections = new KeyedCollectionWrapper<string, IniSection>(mergedSectionList, x => x.Name, StringComparer.InvariantCultureIgnoreCase);

            foreach (IniSection section in this.Sections)
            {
                if (!mergedSections.ContainsKey(section.Name))
                {
                    mergedSections.Add(new IniSection(section.Name));
                }

                mergedSections[section.Name].Elements.AddRange(section.Elements);
            }

            this.Sections.Clear();
            this.Sections.AddRange(mergedSectionList);
        }

        public void MergeTextElements ()
        {
            foreach (IniSection section in this.Sections)
            {
                List<List<IniSectionElement>> elementsToMerge = new List<List<IniSectionElement>>();
                List<IniSectionElement> currentElementsToMerge = null;

                foreach (IniSectionElement element in section.Elements)
                {
                    if (element is IniTextElement)
                    {
                        if (currentElementsToMerge == null)
                        {
                            currentElementsToMerge = new List<IniSectionElement>();
                            elementsToMerge.Add(currentElementsToMerge);
                        }

                        currentElementsToMerge.Add(element);
                    }
                    else
                    {
                        currentElementsToMerge = null;
                    }
                }

                foreach (List<IniSectionElement> elementToMerge in elementsToMerge)
                {
                    if (elementToMerge.Count <= 1)
                    {
                        continue;
                    }

                    IniTextElement mergedElement = new IniTextElement();

                    section.Elements[section.Elements.IndexOf(elementToMerge[0])] = mergedElement;
                    section.Elements.RemoveRangeAll(elementToMerge);

                    StringBuilder sb = new StringBuilder();

                    foreach (IniTextElement textToMerge in elementToMerge)
                    {
                        if (sb.Length > 0)
                        {
                            sb.AppendLine();
                        }

                        sb.Append(textToMerge.Text);
                    }

                    mergedElement.Text = sb.ToString();
                }

                List<int> inserts = new List<int>();

                for (int i1 = 0; i1 < section.Elements.Count; i1++)
                {
                    if (!( section.Elements[i1] is IniTextElement ))
                    {
                        if (i1 == section.Elements.Count - 1)
                        {
                            inserts.Add(i1 + 1);
                        }

                        if (i1 == 0)
                        {
                            inserts.Add(i1);
                        }
                        else if (!( section.Elements[i1 - 1] is IniTextElement ))
                        {
                            inserts.Add(i1);
                        }
                    }
                }

                inserts.Sort();
                inserts.Reverse();

                foreach (int index in inserts)
                {
                    section.Elements.Insert(index, new IniTextElement());
                }

                foreach (IniSectionElement element in section.Elements)
                {
                    if (element is IniTextElement)
                    {
                        IniTextElement textElement = (IniTextElement)element;

                        if (!textElement.Text.IsEmpty() && !textElement.Text.EndsWith(Environment.NewLine, StringComparison.InvariantCultureIgnoreCase))
                        {
                            textElement.Text += Environment.NewLine;
                        }
                    }
                }
            }
        }

        public void Normalize (IniDocumentNormalizeOptions options)
        {
            if (( options & IniDocumentNormalizeOptions.RemoveTextElements ) == IniDocumentNormalizeOptions.RemoveTextElements)
            {
                this.RemoveTextElements();
            }

            if (( options & IniDocumentNormalizeOptions.RemoveEmptySections ) == IniDocumentNormalizeOptions.RemoveEmptySections)
            {
                this.RemoveEmptySections(( options & IniDocumentNormalizeOptions.KeepTextInEmptySections ) == IniDocumentNormalizeOptions.KeepTextInEmptySections);
            }

            if (( options & IniDocumentNormalizeOptions.MergeSections ) == IniDocumentNormalizeOptions.MergeSections)
            {
                this.MergeSections();
            }

            if (( options & IniDocumentNormalizeOptions.SortSections ) == IniDocumentNormalizeOptions.SortSections)
            {
                this.Sections.Sort();
            }

            this.MergeTextElements();

            if (( options & IniDocumentNormalizeOptions.SortElements ) == IniDocumentNormalizeOptions.SortElements)
            {
                foreach (IniSection section in this.Sections)
                {
                    //section.Elements.Sort();

                    List<KeyValuePair<IniValueElement, IniTextElement>> tuples = new List<KeyValuePair<IniValueElement, IniTextElement>>();

                    for (int i1 = 1; i1 < section.Elements.Count; i1 += 2)
                    {
                        tuples.Add(new KeyValuePair<IniValueElement, IniTextElement>((IniValueElement)section.Elements[i1], (IniTextElement)section.Elements[i1 - 1]));
                    }

                    tuples.Sort((x, y) =>
                    {
                        return string.Compare(x.Key.Key, y.Key.Key, StringComparison.InvariantCultureIgnoreCase);
                    });

                    section.Elements.Clear();

                    foreach (KeyValuePair<IniValueElement, IniTextElement> tuple in tuples)
                    {
                        section.Elements.Add(tuple.Key);

                        if (!tuple.Value.Text.IsEmpty())
                        {
                            section.Elements.Add(tuple.Value);
                        }
                    }
                }
            }
        }

        public void RemoveElements (IniSectionElementPredicate predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            foreach (IniSection section in this.Sections)
            {
                List<IniSectionElement> elementsToRemove = new List<IniSectionElement>();

                foreach (IniSectionElement element in section.Elements)
                {
                    if (predicate(section, element))
                    {
                        elementsToRemove.Add(element);
                    }
                }

                section.Elements.RemoveRangeAll(elementsToRemove);
            }
        }

        public void RemoveEmptySections ()
        {
            this.RemoveEmptySections(false);
        }

        public void RemoveEmptySections (bool keepTextElements)
        {
            this.RemoveSections(s => s.IsEmpty(keepTextElements));
        }

        public void RemoveSections (IniSectionPredicate predicate)
        {
            if (predicate == null)
            {
                throw new ArgumentNullException(nameof(predicate));
            }

            List<IniSection> sectionsToRemove = new List<IniSection>();

            foreach (IniSection section in this.Sections)
            {
                if (predicate(section))
                {
                    sectionsToRemove.Add(section);
                }
            }

            this.Sections.RemoveRange(sectionsToRemove);
        }

        public void RemoveTextElements ()
        {
            this.RemoveElements((s, e) => e is IniTextElement);
        }

        public void Save (IniWriter writer)
        {
            if (writer == null)
            {
                throw new ArgumentNullException(nameof(writer));
            }

            if (this.Sections.ContainsSection(string.Empty))
            {
                writer.Write(this.Sections[string.Empty]);
            }

            foreach (IniSection section in this.Sections)
            {
                if (string.Equals(section.Name, string.Empty, StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                writer.Write(section);
            }
        }

        public void Save (TextWriter writer)
        {
            this.Save(new IniWriter(writer, true));
        }

        public void Save (StringBuilder stringBuilder)
        {
            if (stringBuilder == null)
            {
                throw new ArgumentNullException(nameof(stringBuilder));
            }

            using (StringWriter w = new StringWriter(stringBuilder, CultureInfo.InvariantCulture))
            {
                this.Save(w);
            }
        }

        public void Save (Stream stream)
        {
            this.Save(new IniWriter(stream, true));
        }

        public void Save (Stream stream, Encoding encoding)
        {
            this.Save(new IniWriter(stream, true, encoding));
        }

        public void Save (Stream stream, Encoding encoding, int bufferSize)
        {
            this.Save(new IniWriter(stream, true, encoding, bufferSize));
        }

        public IDictionary<string, IDictionary<string, IList<string>>> ToDictionaryMultipleValues (IniMultiValueHandling multiValueHandling)
        {
            IDictionary<string, IDictionary<string, IList<string>>> dictionarySections = new Dictionary<string, IDictionary<string, IList<string>>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (IniSection section in this.Sections)
            {
                IDictionary<string, IList<string>> dictionaryElements = new Dictionary<string, IList<string>>(StringComparer.InvariantCultureIgnoreCase);

                foreach (IniSectionElement element in section.Elements)
                {
                    if (!( element is IniValueElement ))
                    {
                        continue;
                    }

                    IniValueElement valueElement = (IniValueElement)element;

                    if (!dictionaryElements.ContainsKey(valueElement.Key))
                    {
                        dictionaryElements.Add(valueElement.Key, new List<string>());
                    }

                    dictionaryElements[valueElement.Key].Add(valueElement.Value);
                }

                if (dictionarySections.ContainsKey(section.Name))
                {
                    IDictionary<string, IList<string>> existingSection = dictionarySections[section.Name];

                    foreach (KeyValuePair<string, IList<string>> dictionaryElement in dictionaryElements)
                    {
                        IList<string> container = null;
                        if (existingSection.ContainsKey(dictionaryElement.Key))
                        {
                            container = existingSection[dictionaryElement.Key];
                        }
                        else
                        {
                            container = new List<string>();
                            existingSection.Add(dictionaryElement.Key, container);
                        }

                        container.AddRange(dictionaryElement.Value);
                    }
                }
                else
                {
                    dictionarySections.Add(section.Name, dictionaryElements);
                }
            }

            if (multiValueHandling == IniMultiValueHandling.All)
            {
                return dictionarySections;
            }

            foreach (KeyValuePair<string, IDictionary<string, IList<string>>> dictionarySection in dictionarySections)
            {
                foreach (KeyValuePair<string, IList<string>> dictionaryElements in dictionarySection.Value)
                {
                    switch (multiValueHandling)
                    {
                        case IniMultiValueHandling.None:
                        {
                            if (dictionaryElements.Value.Count > 1)
                            {
                                dictionaryElements.Value.Clear();
                            }

                            break;
                        }

                        case IniMultiValueHandling.First:
                        {
                            if (dictionaryElements.Value.Count > 1)
                            {
                                string value = dictionaryElements.Value[0];
                                dictionaryElements.Value.Clear();
                                dictionaryElements.Value.Add(value);
                            }

                            break;
                        }

                        case IniMultiValueHandling.Last:
                        {
                            if (dictionaryElements.Value.Count > 1)
                            {
                                string value = dictionaryElements.Value[dictionaryElements.Value.Count - 1];
                                dictionaryElements.Value.Clear();
                                dictionaryElements.Value.Add(value);
                            }

                            break;
                        }
                    }
                }
            }

            return dictionarySections;
        }

        public IDictionary<string, IDictionary<string, string>> ToDictionarySingleValue (IniMultiValueHandling multiValueHandling)
        {
            if (multiValueHandling == IniMultiValueHandling.All)
            {
                throw new ArgumentOutOfRangeException(nameof(multiValueHandling));
            }

            IDictionary<string, IDictionary<string, IList<string>>> dictionarySections = this.ToDictionaryMultipleValues(multiValueHandling);

            IDictionary<string, IDictionary<string, string>> sections = new Dictionary<string, IDictionary<string, string>>(StringComparer.InvariantCultureIgnoreCase);

            foreach (KeyValuePair<string, IDictionary<string, IList<string>>> dictionarySection in dictionarySections)
            {
                IDictionary<string, string> section = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);

                foreach (KeyValuePair<string, IList<string>> dictionaryElements in dictionarySection.Value)
                {
                    section.Add(dictionaryElements.Key, dictionaryElements.Value.Count == 0 ? null : dictionaryElements.Value[0]);
                }

                sections.Add(dictionarySection.Key, section);
            }

            return sections;
        }

        #endregion




        #region Virtuals

        protected virtual void GetObjectData (SerializationInfo info, StreamingContext context)
        {
            info.AddValue(IniDocument.DocumentSerializationName, this.ToString());
        }

        #endregion




        #region Overrides

        public override string ToString ()
        {
            StringBuilder sb = new StringBuilder();

            this.Save(sb);

            return sb.ToString();
        }

        #endregion




        #region Interface: ICloneable<IniDocument>

        object ICloneable.Clone ()
        {
            return this.Clone();
        }

        public virtual IniDocument Clone ()
        {
            IniDocument clone = new IniDocument();
            clone.Sections.AddRange(this.Sections.CloneDeep());
            return clone;
        }

        #endregion




        #region Interface: ISerializable

        void ISerializable.GetObjectData (SerializationInfo info, StreamingContext context)
        {
            if (info == null)
            {
                throw new ArgumentNullException(nameof(info));
            }

            this.GetObjectData(info, context);
        }

        #endregion
    }
}
