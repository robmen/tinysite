---
layout: master
---
{{#with book}}<div class="toc">
  <p>Table of Contents</p>
  <div class="book" style="float:left">
    <ul>
    {{#each chapters}}<li class="chapter{{#if childActive}} child-active{{/if}}{{#active}} active{{/active}}">{{#unless active}}<a href='{{document.url}}'>{{/unless}}{{document.title}}{{#unless active}}</a>{{/unless}}{{#if children}}
      <ul>{{#children}}
      <li class="{{#if chapter}}chapter{{^}}page{{/if}}{{#childActive}} child-active{{/childActive}}{{#active}} active{{/active}}">{{#unless active}}<a href='{{document.url}}'>{{/unless}}{{document.title}}{{#unless active}}</a>{{/unless}}</li>
      {{/children}}</ul>
      {{/if}}</li>{{/each}}
    </ul>
  </div>
</div>{{/with}}

<div class="content">
{{{document.content}}}
</div>