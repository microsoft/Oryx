<template>
  <div class="home">
    <h1>What Can I Deploy to Static Apps?</h1>
    <div class="card-grid">
      <SmallCard
        v-bind:key="project.id"
        v-bind:title="project.name"
        v-bind:slug="project.slug"
        v-bind:id="project.id"
        v-for="project in projects"
      />
    </div>
  </div>
</template>

<script>
import Logo from "~/components/Logo.vue";
import { ReactIcon } from "~/components/Icons.vue";
import SmallCard from "~/components/SmallCard.vue";
import { projects } from "../utils/projectsData";
export default {
  data() {
    return { projects };
  },
  created() {
    // console.log(projects);
  },
  components: {
    Logo,
    ReactIcon,
    SmallCard
  }
};
</script>

<style>
.container {
  margin: 0 auto;
  min-height: 100vh;
  display: flex;
  justify-content: center;
  align-items: center;
  text-align: center;
}

.title {
  font-family: "Quicksand", "Source Sans Pro", -apple-system, BlinkMacSystemFont,
    "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;
  display: block;
  font-weight: 300;
  font-size: 100px;
  color: #35495e;
  letter-spacing: 1px;
}

.subtitle {
  font-weight: 300;
  font-size: 42px;
  color: #526488;
  word-spacing: 5px;
  padding-bottom: 15px;
}

.links {
  padding-top: 15px;
}
</style>
