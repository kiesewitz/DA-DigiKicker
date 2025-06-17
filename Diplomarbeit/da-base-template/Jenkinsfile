pipeline {
    agent { label 'docker-lehrer' }

    options {
        buildDiscarder(logRotator(artifactNumToKeepStr: '0'))
        skipDefaultCheckout(true)
    }
    
    parameters {
        string(
            name: 'REPOSITORY', 
            defaultValue: 'https://github.com/bitsneak/HTLLE-DA-Vorlage.git',
            description: 'URL git-Repository DA//'
        )
        string(
            name: 'GIT_PATH',
            defaultValue: './',
            description: "Pfad zur DA in git (z.B.: dipl/)"
        )
        string(
            name: 'GIT_BRANCH',
            defaultValue: '*/main',
            description: 'Experte: Wenn Sie einen anderen als den \'main\' Branch bauen möchten' 
        )
        choice(
            name: 'ARCHIVE_FORMAT',
            choices: ['zip', 'tar.gz'],
            description: 'Archiv-Typ als der die Arbeit per Mail versandt wird')
    }
    
    stages {
        stage('Set some variables and clean workspace'){
            steps{
                cleanWs()
                script {
                    // set template values
                    env.TEMPLATE_URL = 'https://github.com/bitsneak/HTLLE-DA-Vorlage.git'
                    env.TEMPLATE_NAME = 'HTLLE-DA-Vorlage'

                    // builduser credentials id in jenkins store for gitea login
                    env.GIT_CRED_ID = 'd65d903b-21ee-4055-98aa-ef82a903e287'
                    
                    // add https to the url if not present
                    env.REPOSITORY = params.REPOSITORY
                    if (!env.REPOSITORY.startsWith("https://")) {
                        env.REPOSITORY = "https://" + env.REPOSITORY
                    }
                    // sanitize git dir path
                    env.GIT_PATH = params.GIT_PATH
                    if (env.GIT_PATH.endsWith("/")) {
                        env.GIT_PATH = env.GIT_PATH.substring(
                            0, (env.GIT_PATH.length() - 1))
                    }
                    if (env.GIT_PATH.startsWith("/")) {
                        env.GIT_PATH = env.GIT_PATH.substring(1)
                    }
                    if (env.GIT_PATH.startsWith("~/")) {
                        env.GIT_PATH = env.GIT_PATH.substring(2)
                    }
                    if (env.GIT_PATH.contains("../")) {
                        error("GIT_PATH must not contain '../'")
                    }
                    
                    // be sure branch is set
                    env.GIT_BRANCH = params.GIT_BRANCH ?: '*/main'

                    // output archive filename
                    env.ARCHIVE_FILENAME = 'diplomarbeit.' + params.ARCHIVE_FORMAT

                    // if following files exist add them to the output archive
                    env.ARCHIVE_FILES = 'diplomarbeit.pdf diplomarbeit.pdf.log spellcheck-results.txt'
                }
            }
        }
        stage('Checkout DA') {
            when {
                expression { env.TEMPLATE_URL != env.REPOSITORY }
            }
            steps {
                // checkout out the repository including submodules
                // builduser acc used in git
                checkout([
                    $class: 'GitSCM',
                    branches: [[name: "${env.GIT_BRANCH}"]],
                    doGenerateSubmoduleConfigurations: false,
                    extensions: [[
                        $class: 'SubmoduleOption',
                        disableSubmodules: true,
                        parentCredentials: true,
                        recursiveSubmodules: false,
                        shallow: true,
                        reference: '',
                        trackingSubmodules: false
                    ]],
                    submoduleCfg: [],
                    userRemoteConfigs: [[
                        credentialsId: "${env.GIT_CRED_ID}",
                        url: "${env.REPOSITORY}"
                        ]]
                ])
            }
        }
        stage('Checkout template') {
            steps {
                // remove template folder and fresh checkout
                sh "rm -rf ${env.GIT_PATH}/${env.TEMPLATE_NAME}"
                checkout([
                    $class: 'GitSCM', 
                    branches: [[name: '*/main']], 
                    doGenerateSubmoduleConfigurations: false, 
                    extensions: [[
                        $class: 'RelativeTargetDirectory', 
                        relativeTargetDir: "${env.GIT_PATH}/${env.TEMPLATE_NAME}"
                    ]], 
                    submoduleCfg: [], 
                    userRemoteConfigs: [[
                        credentialsId: "${env.GIT_CRED_ID}", 
                        url: "${env.TEMPLATE_URL}"
                    ]]
                ])
            }
        }
        stage('Build DA') {
            when {
                expression { env.TEMPLATE_URL != env.REPOSITORY }
            }
            steps {
                dir(env.GIT_PATH) {
                    sh"""#!/bin/bash
                    make pdf -C ${env.TEMPLATE_NAME} SOURCEDIR=`pwd`
                    make spellcheck -C ${env.TEMPLATE_NAME} SOURCEDIR=`pwd`
                    """
                }
            }
        }
        stage('Build only template') {
            when {
                expression { env.TEMPLATE_URL == env.REPOSITORY }
            }
            steps {
                // build the template with the examples from DA point of view
                sh """#!/bin/bash
                cp -rv ${env.TEMPLATE_NAME}/example/. .
                make pdf -C ${env.TEMPLATE_NAME} SOURCEDIR=`pwd`
                """
            }
        }
        stage('Test if diplomarbeit.pdf exists') {
            steps {
                dir(env.GIT_PATH) {
                  sh "test -f diplomarbeit.pdf"
                }
            }
        }
        stage('Create Archive') {
            steps {
                dir(env.GIT_PATH) {
                    script {
                        // only include actually created files to the archive
                        env.FILES_TO_INCLUDE = ''
                        def include = env.ARCHIVE_FILES.split(' ').each { filename ->
                            if (fileExists(filename)) {
                                env.FILES_TO_INCLUDE += (filename + ' ')
                            }
                        }

                        if (env.ARCHIVE_FORMAT == 'zip') {
                            sh "zip -q ${env.ARCHIVE_FILENAME} ${env.FILES_TO_INCLUDE}"
                        } else {
                            sh "tar -czf ${env.ARCHIVE_FILENAME} ${env.FILES_TO_INCLUDE}"
                        }
                    }
                }
            }
        }
    }
    post {
        always {
            script {
                env.RECIPIENTS = ""
                // get mail addresses
                // needs script approval in jenkins
                def metadata = readFile(file: "${env.GIT_PATH}/metadata.yaml").split('\n').each { line ->
                    if (line.contains("- build-notification:")) {
                        // remove yaml comments
                        if (line.contains('#')) {
                            line = line.substring(0, line.indexOf('#'))
                        }
                        def mailMatch = line =~ /[_A-Za-z0-9-]+(.[_A-Za-z0-9-]+)*@[A-Za-z0-9]+(.[A-Za-z0-9]+)*(.[A-Za-z]{2,})/
                        if (mailMatch) {
                            env.RECIPIENTS += (mailMatch[0][0] + ";")
                        }
                    }
                }
            }
            dir(env.GIT_PATH) {
                emailext attachmentsPattern: "${env.ARCHIVE_FILENAME}",
                    to: "${env.RECIPIENTS}",
                    subject: "[${currentBuild.currentResult}] Diplomarbeit Build #${env.BUILD_NUMBER}",
                    body: "Job ${env.JOB_NAME}: ${env.JOB_URL}"
            }
        }
    }
}
