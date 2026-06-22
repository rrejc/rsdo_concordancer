import i18n from 'i18next';
import { initReactI18next } from 'react-i18next';

const resources = {
    en: {
        translation: {
            shared: {
                'concordance': 'Search',
                'countSentences': 'sentences',
                'countTexts': 'texts',
                'countWords': 'words',
                'enterSearch': 'Enter search query',
                'export': 'Export',
                'exportFirst': 'Export first',
                'exportMax': 'You can export up to {{max}} records!',
                'exportRandom': 'Export random',
                'exportRecords': 'records.',
                'exportTitle': 'Export data',
                'exportTotal': 'The number of records is {{total}}.',
                'list': 'List',
                'noResults': 'No results were found for your search.',
                'searchError': 'Server returned error when searching.',
                'searchHelp': 'Help',
                'searchHistory': 'Search history',
                'searchRecords': 'Showing {{start}}-{{end}} from {{total}} records.',
                'select': 'Select',
                'unselect': 'Remove selected'
            },
            concordance: {
                'title': 'Search concordances',
                'detailsAnnotation': 'Corpus annotation',
                'detailsAnnotationBasicForm': 'Basic form',
                'detailsAnnotationPos': 'Morphological properties',
                'detailsAnnotationWord': 'Word',
                'detailsParagraph': 'Paragraph',
                'detailsTextAuthor': 'Author',
                'detailsTextSource': 'Source',
                'detailsTextTitle': 'Title',
                'detailsTextYear': 'Year',
                'aggregationLemmas': 'Basic forms',
                'aggregationText': 'Source'
            },
            list: {
                'title': 'Search list'
            }
        }
    },
    sl: {
        translation: {
            shared: {
                'concordance': 'Iskanje',
                'countSentences': 'stavkov',
                'countTexts': 'besedil',
                'countWords': 'besed',
                'enterSearch': 'Vpišite iskalni niz',
                'export': 'Izvozi',
                'exportFirst': 'Izvozi prvih',
                'exportMax': 'Izvozite lahko največ {{max}} zapisov!',
                'exportRandom': 'Izvozi naključnih',
                'exportRecords': 'zapisov.',
                'exportTitle': 'Izvoz podatkov',
                'exportTotal': 'Število zapisov je {{total}}.',
                'list': 'Seznami',
                'noResults': 'Vaše iskanje ni obrodilo sadov.',
                'searchError': 'Strežnik je vrnil napako pri iskanju.',
                'searchHelp': 'Pomoč',
                'searchHistory': 'Zgodovina iskanj',
                'searchRecords': 'Prikazujem {{start}}-{{end}} od {{total}} zapisov.',
                'select': 'Izberi',
                'unselect': 'Odstrani izbrano'
            },
            concordance: {
                'title': 'Iskanje po konkordancah',
                'detailsAnnotation': 'Korpusne oznake',
                'detailsAnnotationBasicForm': 'Osnovna oblika',
                'detailsAnnotationPos': 'Oblikoslovne značilnosti osnovne oblike',
                'detailsAnnotationWord': 'Beseda',
                'detailsParagraph': 'Prikaz odstavka',
                'detailsTextAuthor': 'avtor',
                'detailsTextSource': 'vir',
                'detailsTextTitle': 'naslov',
                'detailsTextYear': 'leto nastanka',
                'aggregationLemmas': 'Osnovne oblike',
                'aggregationText': 'Vir'
            },
            list: {
                'title': 'Iskanje po seznamih'
            }
        }
    }
};

i18n
    .use(initReactI18next)
    .init({
        resources,
        lng: 'sl',
        interpolation: {
            escapeValue: false
        }
    });

export default i18n;
