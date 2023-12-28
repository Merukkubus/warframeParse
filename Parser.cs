using AngleSharp;
using AngleSharp.Dom;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using warframeParse.Models.Entities;

namespace warframeParse
{
    public class Parser
    {
        public static async Task Parse(string url)
        {
            var context = BrowsingContext.New(Configuration.Default.WithDefaultLoader());
            var doc = await context.OpenAsync(url);
            var warframeUrl = doc.Origin + doc.QuerySelector(".home-tile-link a[title = \"Warframes\"]").GetAttribute("href");
            doc = await context.OpenAsync(warframeUrl);
            var warframesUrl = doc.QuerySelectorAll("div.WarframeNavBox div:not(.WarframeNavBoxImage2) a").Select(elem => doc.Origin + elem.GetAttribute("href")).ToList();
            warframe wf;
            Polarity p;
            IElement value;
            bool isPrime = false;
            bool err = false;
            using (var db = new warEntities())
            {
                var wfList = db.warframe.ToList();
                var elList = new List<element>();
                var pList = new List<Polarity>();
                List<Polarity> pl;
                int i = 0;
                while ( i < warframesUrl.Count())
                {
                    doc = await context.OpenAsync(warframesUrl[i]);
                    var temp = doc.QuerySelectorAll("aside[role=\"region\"] section h2").Where(x => x.InnerHtml == "General Information");
                    value = doc.QuerySelector("span.mw-page-title-main");
                    if (isPrime)
                    {
                        value.TextContent += " Prime";
                    }
                    if (!wfList.Select(x => x.name).Contains(value.TextContent))
                    {
                        wf = new warframe();
                        wf.id = Guid.NewGuid();
                        wf.name = value.TextContent;
                        if (!isPrime)
                        {
                            value = doc.QuerySelectorAll("aside[role=\"region\"] section h2").Where(x => x.InnerHtml == "General Information").ElementAt(0).GetAncestor<IElement>();
                        }
                        else
                        {
                            try
                            {
                                value = doc.QuerySelectorAll("aside[role=\"region\"] section h2").Where(x => x.InnerHtml == "General Information").ElementAt(1).GetAncestor<IElement>();

                            }
                            catch
                            {
                                err = true;
                                value = doc.QuerySelectorAll("aside[role=\"region\"] section h2").Where(x => x.InnerHtml == "General Information").ElementAt(0).GetAncestor<IElement>();
                            }
                        }
                        foreach (var prop in value.QuerySelectorAll("div h3"))
                        {
                            switch (prop.TextContent)
                            {
                                case "Sex":
                                    wf.sex = prop.GetAncestor<IElement>().QuerySelector("div").TextContent;
                                    break;
                                case "Health":
                                    wf.health = int.Parse(prop.GetAncestor<IElement>().QuerySelector("div").TextContent.Split(' ')[0]);
                                    break;
                                case "Shields":
                                    wf.shields = int.Parse(prop.GetAncestor<IElement>().QuerySelector("div").TextContent.Split(' ')[0]);
                                    break;
                                case "Armor":
                                    wf.armor = int.Parse(prop.GetAncestor<IElement>().QuerySelector("div").TextContent.Split(' ')[0]);
                                    break;
                                case "Energy":
                                    wf.energy = int.Parse(prop.GetAncestor<IElement>().QuerySelector("div").TextContent.Split(' ')[0]);
                                    break;
                                case "Sprint Speed":
                                    wf.sprint_speed = double.Parse(prop.GetAncestor<IElement>().QuerySelector("div").TextContent, new CultureInfo("en-us"));
                                    break;
                                case "Progenitor Element":
                                    if (!err)
                                    {
                                        var elem = prop.GetAncestor<IElement>().QuerySelector("div span span").TextContent.Trim();
                                        wf.element = elList.FirstOrDefault(x => x.name == elem);
                                        if (wf.element == null)
                                        {
                                            wf.element = new element
                                            {
                                                id = Guid.NewGuid(),
                                                name = elem
                                            };
                                            db.element.Add(wf.element);
                                            elList.Add(wf.element);
                                        }
                                    }
                                    break;
                                case "Polarities":
                                    if (!err)
                                    {
                                        var ancestor = prop.GetAncestor<IElement>();
                                        var imgAlts = ancestor.QuerySelectorAll("div img").Select(m => m.GetAttribute("alt").Split(' ')[0]);
                                        foreach (var alt in imgAlts)
                                        {
                                            if (!pList.Any(y => y.name == alt))
                                            {
                                                p = new Polarity
                                                {
                                                    id = Guid.NewGuid(),
                                                    name = alt
                                                };
                                                pList.Add(p);
                                                db.Polarity.Add(p);
                                            }
                                        }
                                        pl = prop.GetAncestor<IElement>().QuerySelectorAll("div img").Select(m => m.GetAttribute("alt").Split(' ')[0]).Select(x => pList.FirstOrDefault(y => y.name == x)).ToList();
                                        foreach (var x in pl)
                                        {
                                            var wfod = wf.warframe_polarity.FirstOrDefault(g => g.warframe_id == wf.id && g.polarity_id == x.id);
                                            if (wfod != null)
                                            {
                                                wfod.quantity++;
                                            }
                                            else
                                            {
                                                wf.warframe_polarity.Add(new warframe_polarity
                                                {
                                                    warframe_id = wf.id,
                                                    polarity_id = x.id,
                                                    quantity = 1
                                                });
                                            }
                                        };
                                    }
                                    break;
                            }
                        }
                        if (!err)
                        {
                            db.warframe.Add(wf);
                        }
                    }
                    if (doc.QuerySelector("li[data-hash = \"Prime\"]") != null && !isPrime)
                    {
                        warframesUrl[i] += "#Prime";
                        isPrime = true;
                        continue;
                    }
                    else
                    {
                        isPrime = false;
                        i++;
                    }
                    err = false;
                }
                db.SaveChanges();
            }
            MessageBox.Show("Данные получены");
        }
    }
}