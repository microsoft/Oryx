<template>
  <a class="card-small" v-bind:href="href">
    <!-- <Icon w="{153}" h="{163}" /> -->
    <component v-bind:is="id"></component>
    <h3>{{ title }}</h3>
  </a>
</template>

<script>
import projectIcons from "./Icons";
import { projects } from "../utils/projectsData";

const validComponents = {};

Object.keys(projectIcons).forEach(key => {
  if (!key.startsWith("_")) {
    validComponents[key] = projectIcons[key];
  }
});

export default {
  props: ["title", "slug", "id"],
  data() {
    return {};
  },
  components: { ...validComponents },
  computed: {
    href: function() {
      return `/project/${this.slug}`;
    }
  }
};
</script>

<style></style>
