const statOptions = [
	{
		category: "Classic",
		icon: "lightning-bolt-icon",
		options: ["Charisma", "Constitution", "Dexterity", "Intelligence", "Perception", "Speed", "Strength", "Wisdom"],
	},
	{
		category: "Industries",
		icon: "briefcase-icon",
		options: ["Fintech", "HealthTech", "InfoSec", "Proptech", "Retail"],
	},
	{
		category: "Startup Skills",
		icon: "lightbulb-icon",
		options: [
			"Architecture",
			"Bootstrapping",
			"Business Development",
			"Business Operations",
			"Data Analysis",
			"DevOps",
			"Finances",
			"Investor Relations",
			"Marketing",
			"Machine Learning",
			"Networking",
			"Passion",
			"Patience",
			"Product",
			"Purpose",
			"Resilience",
			"Sales",
			"Security",
			"Shipping",
			"Strategy",
		],
	},
	{
		category: "Technology",
		icon: "floppy-icon",
		options: [
			".NET",
			"Azure",
			"C",
			"C#",
			"C++",
			"Clojure",
			"Containers",
			"Debian",
			"Docker",
			"Elixir",
			"Erlang",
			"F#",
			"Golang",
			"Java",
			"JavaScript",
			"Kafka",
			"Kubernetes",
			"Linux",
			"NodeJS",
			"PHP",
			"Python",
			"Rails",
			"React",
			"Ruby",
			"Rust",
			"Scala",
			"Serverless",
			"Tensorflow",
			"Ubuntu",
			"Vue",
			"Windows",
		],
	},
];

export const resolveIcon = (category: string): string => {
	try {
		return statOptions.filter(opt => opt && opt.category === category)[0].icon;
	} catch (e) {
		return "";
	}
};

export default statOptions;
