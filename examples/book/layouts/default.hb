---
layout: master
---
{{#with document.book}}<div class="toc">
  <p>Table of Contents</p>
  <div class="book">
    <ul>
    {{#each chapters}}<li class="chapter{{#if subPageActive}} child-active{{/if}}{{#active}} active{{/active}}">{{#unless active}}<a href='{{document.url}}'>{{/unless}}{{document.title}}{{#unless active}}</a>{{/unless}}{{#if subPages}}
      <ul>{{#subPages}}
      <li class="{{#if chapter}}chapter{{^}}page{{/if}}{{#subPageActive}} child-active{{/subPageActive}}{{#active}} active{{/active}}">{{#unless active}}<a href='{{document.url}}'>{{/unless}}{{document.title}}{{#unless active}}</a>{{/unless}}</li>
      {{/subPages}}</ul>
      {{/if}}</li>{{/each}}
    </ul>
  </div>
</div>{{/with}}

<div class="content">
{{{document.content}}}
</div>