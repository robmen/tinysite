<?xml version="1.0" encoding="utf-8" standalone="yes"?>
<feed xmlns="http://www.w3.org/2005/Atom">
  <title>{{site.title}} - {{site.subtitle}}</title>
  <author>
    <name>{{site.author.name}}</name>
    <email>{{site.author.email}}</email>
    <uri>{{site.author.url}}</uri>
  </author>
  <id>{{document.fullUrl}}</id>
  <link rel="alternate" type="type/html" href="{{site.fullUrl}}" />
  <link rel="self" type="application/atom+xml" href="{{document.fullUrl}}" />
  <updated>{{document.nowStandardUtcDate}}</updated>
{{#document.paginator.documents}}
  <entry>
    <id>{{fullUrl}}</id>
    <author>
      <name>{{author.name}}</name>
      <email>{{author.email}}</email>
      <uri>{{author.url}}</uri>
    </author>
    <title>{{title}}</title>
    <summary type="html">{{summary}}</summary>{{#tags}}
    <category term="{{this}}" />
    {{/tags}}
    <link rel="alternate" type="text/html" href="{{fullUrl}}" />
    <content type="html">{{summary}}&lt;a href='{{fullUrl}}'&gt;Read more...&lt;/a&gt;&lt;/p&gt;</content>
    <updated>{{standardUtcDate}}</updated>
  </entry>
{{/document.paginator.documents}}
</feed>
