using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace msra.nlp.tr
{
    class OpenNer : Ner
    {

        opennlp.tools.namefind.NameFinderME locationNameFinder = null;
        opennlp.tools.namefind.NameFinderME personNameFinder = null;
        opennlp.tools.namefind.NameFinderME organizationNameFinder = null;
        List<pml.type.Pair<string, string>> entities = null;

        public OpenNer() { }

        System.Text.RegularExpressions.Regex regex = new System.Text.RegularExpressions.Regex(@"\s+");
        public void FindNer(string context)
        {
            if (locationNameFinder == null)
            {
                Initial();
            }
            var tokens = regex.Split(context);
            // Find location names
            var locations = new List<string>();
            opennlp.tools.util.Span[] locationSpan = locationNameFinder.find(tokens);

            //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            locationNameFinder.clearAdaptiveData();
            locations.AddRange(opennlp.tools.util.Span.spansToStrings(locationSpan, tokens).AsEnumerable());
            // Find person names
            var persons = new List<string>();
            opennlp.tools.util.Span[] personSpan = personNameFinder.find(tokens);

            //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            personNameFinder.clearAdaptiveData();
            persons.AddRange(opennlp.tools.util.Span.spansToStrings(personSpan, tokens).AsEnumerable());
            // Find organization names
            var organizations = new List<string>();
            opennlp.tools.util.Span[] organizationSpan = organizationNameFinder.find(tokens);

            //important:  clear adaptive data in the feature generators or the detection rate will decrease over time.
            organizationNameFinder.clearAdaptiveData();
            organizations.AddRange(opennlp.tools.util.Span.spansToStrings(organizationSpan, tokens).AsEnumerable());

            entities = new List<pml.type.Pair<string, string>>();
            foreach (var location in locations)
            {
                entities.Add(new pml.type.Pair<string, string>(location, "LOCATION"));
            }
            foreach (var person in persons)
            {
                entities.Add(new pml.type.Pair<string, string>(person, "PERSON"));
            }
            foreach (var organization in organizations)
            {
                entities.Add(new pml.type.Pair<string, string>(organization, "ORGANIZATION"));
            }
        }

        public List<pml.type.Pair<string, string>> GetEntities()
        {
            return entities;
        }

        public string GetNerType(string mention)
        {
            if (mention == null)
            {
                throw new Exception("Mention should not be null for finding ner type");
            }
            mention = regex.Replace(mention, "").ToLower();
            foreach (var entity in entities)
            {
                var str1 = regex.Replace(entity.first, "").ToLower();
                if (str1.Contains(mention) || mention.Contains(str1))
                {
                    return entity.second;
                }
            }
            return "UNKNOW";
        }

        private void Initial()
        {
            var basedir = @"D:\Codes\C#\EntityTyping\Fine-ner\input\opennlp models";
            var modelInputStream = new java.io.FileInputStream(Path.Combine(basedir, "en-ner-location.bin")); //load the name model into a stream
            var model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream); //load the model
            locationNameFinder = new opennlp.tools.namefind.NameFinderME(model);                   //create the namefinder
            modelInputStream = new java.io.FileInputStream(Path.Combine(basedir, "en-ner-person.bin"));
            model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream);
            personNameFinder = new opennlp.tools.namefind.NameFinderME(model);
            modelInputStream = new java.io.FileInputStream(Path.Combine(basedir, "en-ner-organization.bin"));
            model = new opennlp.tools.namefind.TokenNameFinderModel(modelInputStream);
            organizationNameFinder = new opennlp.tools.namefind.NameFinderME(model);
        }
    }
}
