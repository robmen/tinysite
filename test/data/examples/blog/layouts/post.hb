---
layout: master
---
<div class="hentry">
  <h2 class="entry-title">{{document.title}}</h2>
  <div entry-meta"><p><a href="{{document.fullUrl}}" rel="bookmark" title="permalink to '{{document.title}}'">#</a> by <span class="vcard author"><a class="url fn" href="{{document.author.url}}">{{document.author.name}}</a></span> on <abbr class="published" title="{{#with document.date}}{{Year}}-{{Month}}-{{Day}}{{/with}}">{{document.friendlyDate}}</abbr></p></div>
  <div entry-content">{{{document.content}}}</div>
  {{#with document.previousDocument}}<div prev-document">Prev: <a href="{{url}}">{{title}}</a></div>{{/with}}
  {{#with document.nextDocument}}<div next-document">Next: <a href="{{url}}">{{title}}</a></div>{{/with}}
</div>
